namespace TaskQueue.Tests;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        // Устанавливаем кодировку UTF-8 для всей программы
//Console.OutputEncoding = Encoding.UTF8;
Console.OutputEncoding = System.Text.Encoding.UTF8;
System.Diagnostics.Trace.Listeners.Clear();

// Добавляем консольный прослушиватель с UTF-8
var consoleListener = new System.Diagnostics.TextWriterTraceListener(Console.Out)
{
    Name = "ConsoleListener"
};
//System.Diagnostics.Trace.Listeners.Add(consoleListener);
        System.Diagnostics.Trace.Listeners.Add(new System.Diagnostics.ConsoleTraceListener());
        
        var messages = Enumerable.Range(1, 1000).Select(i => $"Message-{i}");

        var thread_pool = new TaskQueue(10);
        {
            //thread_pool.Execute("123", obj => { });

            foreach (var message in messages)
                thread_pool.Execute(() =>
                {
                   
                    Console.WriteLine(">> Обработка сообщения {0} начата...", message);
                    Thread.Sleep(100);
                    Console.WriteLine(">> Обработка сообщения {0} выполнена", message);
                });

            Console.ReadLine();
        }


        Console.ReadLine();
    }
}
