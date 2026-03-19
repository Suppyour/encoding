using LzwArchiverApp.Algorithms;

namespace LzwArchiverApp
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Введите команду из доступных:");
                Console.WriteLine("`compress {input_file} [output_file]` - сжимает файл LZW");
                Console.WriteLine("`decompress {input_file} [output_file]` - распаковывает файл LZW");
                
                var input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input)) return;
                args = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            }

            if (args.Length < 2)
            {
                Console.WriteLine("Неверное количество аргументов");
                return;
            }

            var command = args[0].ToLower();
            var inputFile = args[1];

            if (!File.Exists(inputFile))
            {
                Console.WriteLine($"Файл {inputFile} не найден.");
                return;
            }

            var outputFile = args.Length > 2 
                ? args[2] 
                : (command == "compress" ? inputFile + ".compressed" : inputFile + ".decompressed");

            try
            {
                long inputSize = new FileInfo(inputFile).Length;

                if (command == "compress")
                {
                    Console.WriteLine("Сжатие LZW. Работаем...");
                    LzwArchiver.Compress(inputFile, outputFile);
                }
                else if (command == "decompress")
                {
                    Console.WriteLine("Распаковка LZW. Работаем...");
                    LzwArchiver.Decompress(inputFile, outputFile);
                }
                else
                {
                    Console.WriteLine("Неправильная команда. Доступно: compress, decompress");
                    return;
                }

                long outputSize = new FileInfo(outputFile).Length;
                
                Console.WriteLine("Успех!");
                Console.WriteLine($"Исходный размер: {inputSize} байт");
                Console.WriteLine($"Итоговый размер: {outputSize} байт");
                if (inputSize > 0)
                {
                    double ratio = (double)outputSize / inputSize * 100;
                    Console.WriteLine($"Коэффициент (Изменение): {ratio:F2}% от исходного размера.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Ошибка: {e.Message}");
                Console.WriteLine(e.StackTrace);
            }
        }
    }
}
