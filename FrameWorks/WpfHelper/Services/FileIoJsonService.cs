using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace WpfHelper.Services
{
    public static class FileIoJsonService<T>
    {
        /// <summary>
        /// Reads a json file and deserializes it
        /// </summary>
        /// <param name="path"></param>
        /// <exception cref="InvalidOperationException"></exception>
        /// <returns></returns>
        public static T Read(string path)
        {
            FileChecks(path);

            string json = File.ReadAllText(path);
            if (string.IsNullOrEmpty(json))
                if (typeof(T) == typeof(string))
                    return (T)(object)string.Empty;
                else
                    return (T)(Activator.CreateInstance(typeof(T)) ?? throw new InvalidOperationException("Something went wrong"));

            if (JsonSerializer.Deserialize<T>(json) is not T data)
                return (T)(Activator.CreateInstance(typeof(T)) ?? throw new InvalidOperationException("Something went wrong"));

            return data;
        }

        /// <summary>
        /// Reads a json file async and deserializes it async
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static async Task<T?> ReadAsync(string path)
        {
            FileChecks(path);

            await using FileStream stream = File.OpenRead(path);
            return await JsonSerializer.DeserializeAsync<T>(stream);
        }

        /// <summary>
        /// Saves data to json file
        /// </summary>
        /// <param name="path"></param>
        /// <param name="data"></param>
        public static void Save(string path, T data)
        {
            using StreamWriter write = File.CreateText(path);

            string output = JsonSerializer.Serialize(data);
            write.Write(output);
        }

        /// <summary>
        /// Saves data to json file async
        /// </summary>
        /// <param name="path"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static async Task SaveAsync(string path, T data)
        {
            await using FileStream stream = File.Create(path);
            await JsonSerializer.SerializeAsync(stream, data);
        }

        /// <summary>
        /// Checks for the existence of a file, if it not found file will be created
        /// </summary>
        /// <param name="path"></param>
        private static void FileChecks(in string path)
        {
            if (File.Exists(path)) return;

            File.CreateText(path).Dispose();
        }
    }
}
