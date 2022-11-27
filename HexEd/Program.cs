//A hex editor
//Features:
//select, highlight and name
//copy and paste
//show position
//Show dec value of byte, short, int, long, date, etc


using HexEd;

//var project = new Project { FilePath = "data.bin" };
var project = Project.Load("project.json");
var renderer = new Renderer();
renderer.Clear();
var bytes = await File.ReadAllBytesAsync(project.FilePath);
var charsPerLine = 16;
var selectedIndex = project.Position;
var selectedLength = 0;
while (true)
{
    project.Position = selectedIndex;
    renderer.BeginDraw();
    var xOffset = 10;
    var yOffset = 1;
    Console.SetCursorPosition(0, 0);
    var lineLength = 0;
    var hex = string.Empty;
    var ascii = string.Empty;
    for (int i = 0; i < charsPerLine; i++)
    {
        renderer.PrintAt(i * 3 + 10, 0, i.ToString("x2"), ConsoleColor.DarkGray);
    }

    for (int i = 1; i < Console.WindowHeight; i++)
    {
        renderer.PrintAt(0, i, ((i - 1) * charsPerLine).ToString("x8"), ConsoleColor.DarkGray);
    }
    renderer.PrintAt(0, Console.WindowHeight - 1, $"Offset:{selectedIndex:x8}", ConsoleColor.DarkGray);

    renderer.PrintAt(80, 0, $"Binary:{Convert.ToString(bytes[selectedIndex], 2).PadLeft(8, '0')}", ConsoleColor.DarkGray);
    renderer.PrintAt(80, 1, $"SByte :{(sbyte)bytes[selectedIndex]}", ConsoleColor.DarkGray);
    renderer.PrintAt(80, 2, $"Byte  :{bytes[selectedIndex]}", ConsoleColor.DarkGray);
    renderer.PrintAt(80, 3, $"Int16 :{(short)((bytes[selectedIndex] << 8) + bytes[selectedIndex + 1])}", ConsoleColor.DarkGray);
    renderer.PrintAt(80, 4, $"UInt16:{(ushort)((bytes[selectedIndex] << 8) + bytes[selectedIndex + 1])}", ConsoleColor.DarkGray);
    renderer.PrintAt(80, 5, $"Int32 :{(short)bytes[selectedIndex]}", ConsoleColor.DarkGray);
    renderer.PrintAt(80, 6, $"UInt32:{(short)bytes[selectedIndex]}", ConsoleColor.DarkGray);
    renderer.PrintAt(80, 6, $"Int64 :{(short)bytes[selectedIndex]}", ConsoleColor.DarkGray);
    renderer.PrintAt(80, 6, $"UInt64:{(short)bytes[selectedIndex]}", ConsoleColor.DarkGray);


    for (int i = 0; i < bytes.Length; i++)
    {
        hex += bytes[i].ToString("x2") + ' ';
        ascii += bytes[i] > 0x20 ? (char)bytes[i] : '.';
        var chr = bytes[i] > 0x20 ? (char)bytes[i] : '.';
        lineLength++;
        if (yOffset > Console.WindowHeight - 2)
        {
            break;
        }

        Selection selection = null;

        foreach (var s in project.Selections)
        {
            if (s.Start <= i && s.Start + s.Length >= i)
            {
                selection = s;
                break;
            }
        }

        if (i >= selectedIndex && i <= selectedIndex + selectedLength)
        {
            if (selectedIndex + selectedLength > i)
            {
                renderer.PrintAt(xOffset, yOffset, bytes[i].ToString("x2") + " ", selection == null ? ConsoleColor.Black : selection.Color, ConsoleColor.Gray);
            }
            else
            {
                renderer.PrintAt(xOffset, yOffset, bytes[i].ToString("x2"), selection == null ? ConsoleColor.Black : selection.Color, ConsoleColor.Gray);
            }
        }
        else
        {
            renderer.PrintAt(xOffset, yOffset, bytes[i].ToString("x2"), selection == null ? ConsoleColor.White : selection.Color, ConsoleColor.Black);
        }

        renderer.PrintAt(10 + lineLength + 50, yOffset, chr.ToString(), i == selectedIndex ? ConsoleColor.Black : ConsoleColor.White, i == selectedIndex ? ConsoleColor.Gray : ConsoleColor.Black);

        xOffset += 3;
        if (lineLength == charsPerLine)
        {
            xOffset = 10;
            yOffset++;
            lineLength = 0;
        }
    }

    renderer.EndDraw();

    var keyInfo = Console.ReadKey(true);

    if (keyInfo.Key == ConsoleKey.D1)
    {
        project.Selections.Add(new Selection
        {
            Start = selectedIndex,
            Length = selectedLength,
            Color = ConsoleColor.Red
        });
        selectedIndex += selectedLength;
        selectedLength = 0;
    }

    if (keyInfo.Key == ConsoleKey.D2)
    {
        project.Selections.Add(new Selection
        {
            Start = selectedIndex,
            Length = selectedLength,
            Color = ConsoleColor.Blue
        });
        selectedIndex += selectedLength;
        selectedLength = 0;
    }

    if (keyInfo.Key == ConsoleKey.D3)
    {
        project.Selections.Add(new Selection
        {
            Start = selectedIndex,
            Length = selectedLength,
            Color = ConsoleColor.Yellow
        });
        selectedIndex += selectedLength;
        selectedLength = 0;
    }

    if (keyInfo.Key == ConsoleKey.Q)
    {
        break;
    }
    if (keyInfo.Key == ConsoleKey.LeftArrow)
    {
        if ((keyInfo.Modifiers & ConsoleModifiers.Shift) > 0)
        {
            selectedLength = Math.Max(0, selectedLength - 1);
        }
        else
        {
            selectedIndex = Math.Max(0, selectedIndex + selectedLength - 1);
            selectedLength = 0;
        }
    }
    if (keyInfo.Key == ConsoleKey.RightArrow)
    {
        if ((keyInfo.Modifiers & ConsoleModifiers.Shift) > 0)
        {
            selectedLength = Math.Min(bytes.Length - 1 - selectedIndex, selectedLength + 1);
        }
        else
        {
            selectedIndex = Math.Min(bytes.Length - 1, selectedIndex + selectedLength + 1);
            selectedLength = 0;
        }
    }

    if (keyInfo.Key == ConsoleKey.UpArrow)
    {
        selectedIndex = Math.Max(0, selectedIndex - charsPerLine + selectedLength);
        selectedLength = 0;
    }

    if (keyInfo.Key == ConsoleKey.DownArrow)
    {
        selectedIndex = Math.Min(bytes.Length - 1, selectedIndex + charsPerLine + selectedLength);
        selectedLength = 0;
    }
}

project.Save("project.json");