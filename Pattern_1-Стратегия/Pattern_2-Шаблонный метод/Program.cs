﻿/*
 * ПАТТЕРНЫ ПОВЕДЕНИЯ
 * 
 * Глава_2 и 10: Фабричный метод(виртуальный конструктор) и Шаблонный метод (Паттерн поведения)
 * 
 *  — это каркас, в который наследники могут
 * подставить реализации недостающих элементов.
 * 
 * - позволяет более четко определить «контракт» между базовым классом и потомками
 */

using Pattern_1_Стратегия;
using Pattern_2_Шаблонный_метод;
using System.ServiceModel;
using System.Text;

/*
 * Теперь все реализации читателей логов будут вынуждены следовать согласованному протоколу
 */
public abstract class LogReader
{
    private int _currentPosition;

    //Метод ReadlogEntry невиртуальный: определяет алгоритм импорта
    public IEnumerable<LogEntry> ReadLogEntry()
    {
        return ReadEntries(ref _currentPosition).Select(ParseLogEntry);
    }

    protected abstract IEnumerable<string> ReadEntries(ref int position);

    protected abstract LogEntry ParseLogEntry(string stringEntry);
}

/* 
 * 2.1: Локальный шаблонный метод на основе делегатов
 * 
 * ! является более контекстно зависимой операцией
 * 
 * Использование наследования является слишком тяжеловесным решением, 
 * поэтому в таких случаях применяется подход, при котором переменный шаг алгоритма задается делегатом
 */

//интерфейс сервиса сохранения записей
interface ILogSaver
{
    void UploadLogEntries(IEnumerable<LogEntry>logEntries);
    void UploadExceptions(IEnumerable<ExceptionLogEntry> exceptions);
}

//Прокси - класс инкапсулирует особенности работы
//с WCF - инфраструктурой
class LogSaverProxy : ILogSaver
{
    //подключение к службе 
    class LogSaverClient : ClientBase<ILogSaver>
    {
        public ILogSaver LogSaver
        {
            get { return Channel; }
        }
    }

    public void UploadExceptions(IEnumerable<ExceptionLogEntry> exceptions)
    {
        UseProxyClient(c => c.UploadExceptions(exceptions));
    }

    public void UploadLogEntries(IEnumerable<LogEntry> logEntries)
    {
        UseProxyClient(c => c.UploadLogEntries(logEntries));
    }

    private void UseProxyClient(Action<ILogSaver> accessor)
    {
        var client = new LogSaverClient();
        try
        {
            accessor(client.LogSaver);
            client.Close();
        }
        catch (CommunicationException e)
        {
            client.Abort();
            Console.WriteLine(e.Message);
        }
    }
}
/*
 * Подход на основе делегатов может не только применяться для определения 
 * локальных действий внутри класса, но и передаваться извне другому объекту 
 * в аргументах конструктора.
 */

/*
 * 2.2: Шаблонный метод на основе методов расширения
 */

public abstract class LogEntryBase
{
    public DateTime EntryDateTime { get; internal set; }
    public Severity Severity { get; internal set; }
    public string? Message { get; internal set; }

    //ExceptionLogEntry будет возвращать информацию об исключении
    public string? AdditionalInformation { get; internal set; }
}

public static class LogEntryBaseEx
{
    public static string GetText(this LogEntryBase logEntry)
    {
        var sb = new StringBuilder();

        sb.AppendFormat("{0}", logEntry.EntryDateTime)
            .AppendFormat("{0}", logEntry.Severity)
            .AppendLine(logEntry.Message)
            .AppendLine(logEntry.AdditionalInformation);

        return sb.ToString();
    }
}

//интерфейс для взаимодействия с фабриками
public abstract class Creator
{
    private Product? product;

    public abstract Product ProductFactoryMethod(int x, int y);
    public abstract Product ProductFactoryMethod_2(int x, int y);

    public void AnProductOperation(int x, int y)
    {
        product = ProductFactoryMethod(x, y);
    }

    public void AnProductOperation_2(int x, int y)
    {
        product = ProductFactoryMethod_2(x, y);
    }
}

//конкретная фабрика
public class ConcreteCreator : Creator
{
    public override Product ProductFactoryMethod(int x, int y)
    {
        return new ConcreteProduct(x, y);
    }

    public override Product ProductFactoryMethod_2(int x, int y)
    {
        return new ConcreteProduct_2(x, y);
    }
}

//интерфейс для взаимодействия с продуктами
public abstract class Product
{
    public abstract void GetText();
}

//конкретный продукт
public class ConcreteProduct : Product
{
    public ConcreteProduct(int x, int y)
    {
        Console.WriteLine($"x {x} y {y}");
    }

    public override void GetText()
    {
        Console.WriteLine("1");
    }
}


public class ConcreteProduct_2 : Product
{
    public ConcreteProduct_2(int x, int y)
    {
        Console.WriteLine($"x2 {x} y2 {y}");
    }

    public override void GetText()
    {
        Console.WriteLine("2");
    }
}


public class Program
{
    public static void Main(string[] args)
    {
        Product product1 = null;
        Product product2 = null;
        var creator = new ConcreteCreator(); //Инициализирую фабрику
        product1 = creator.ProductFactoryMethod(1, 2);
        product2 = creator.ProductFactoryMethod_2(1, 2);

        Console.WriteLine($"{product1.GetType().Name} {product2.GetType().Name}");

        product1.GetText();
        product2.GetText();
    }
}