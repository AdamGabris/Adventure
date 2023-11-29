using System.Collections;
using Utils;
using static Utils.Constants;
using Adventure.BuildingBlocks;
using static Utils.Output;
using static Adventure.AssetsAndSettings;


namespace Adventure
{
    public class AdvenureGame : IGameScreen
    {
        const int PADDING = 3;
        int startRow = 5;
        int startColumn = (int)((Console.WindowWidth - MAX_LINE_WIDTH) * 0.5);

        #region  Basic Commands -------------------------------------------------------------------------------

        const string QUIT_COMMAND = "quit";
        const string CLEAR_COMMAND = "clear";
        const string HELP_COMMAND = "help";
        const string LOOK_COMMAND = "look";

        const string DASH = "-";
        const string PROMPT_SYMBOL = "> ";

        Dictionary<string, Action<AdvenureGame>> basicCommands = new Dictionary<string, Action<AdvenureGame>>()
        {
            [QUIT_COMMAND] = (game) => { game.OnExitScreen(typeof(SplashScreen), new object[] { AssetsAndSettings.SPLASH_ART_FILE, true }); },
            [CLEAR_COMMAND] = (game) => { Console.Clear(); },
            [HELP_COMMAND] = (game) => { game.currentDescription = "///TODO: This should print a helpfull message, maybe a list of commands? But it is not."; },
            [LOOK_COMMAND] = (game) => { game.currentDescription = game.currentLocation.Description; }
        };

        #endregion

        string commandBuffer;
        string command;
        string currentDescription = "";
        Location currentLocation;
        bool dirty = true;
        public Action<Type, Object[]> OnExitScreen { get; set; }

        Player hero;
        Inventory playerInventory;

        public void Init()
        {
            command = commandBuffer = String.Empty;
            Adventure.Parser parser = new();
            currentLocation = parser.CreateLocationFromDescription(AssetsAndSettings.GAME_SOURCE);
            currentDescription = currentLocation.Description;
            hero = new Player();
            playerInventory = new Inventory();
        }
        public void Input()
        {
            if (Console.KeyAvailable)
            {
                ProcessKey();
                dirty = true;
            }
        }


        public void Update()
        {
            ///TODO: refactor this function. i.e. make it more readable. 

            if (command != String.Empty)
            {
                if (basicCommands.ContainsKey(command))
                {
                    basicCommands[command](this);
                }
                else
                {
                    string actionDesc = "";
                    string targetDesc = "";

                    string[] commandParts = command.Split(" ", StringSplitOptions.TrimEntries);

                    foreach (string item in commandParts)
                    {
                        if (currentLocation.keywords.Contains(item) && actionDesc == "")
                        {
                            actionDesc = item;
                        }
                        else if (currentLocation.Inventory.Keys.Contains<string>(item) && targetDesc == "")
                        {
                            targetDesc = item;
                        }

                        if (actionDesc != "" && targetDesc != "")
                        {
                            break; // No longer anny point in staying in this for loop. 
                        }
                    }

                    if (targetDesc != "" && actionDesc != "")
                    {
                        Item target = currentLocation.Inventory[targetDesc];
                        string key = $"{target.Status}.{actionDesc}";

                        if (target.actions.Keys.Contains<string>(key))
                        {

                            foreach (string assertion in target.actions[key])
                            {
                                string[] parts = assertion.Split(" => ", StringSplitOptions.TrimEntries);
                                if (parts.Length >= 2)
                                {
                                    string assertionKey = parts[0];
                                    string assertionValue = parts[1];

                                    ///TODO: Remove magick key
                                    if (assertionKey == "Description")
                                    {
                                        currentDescription = assertionValue;
                                    }
                                    else if (assertionKey == "Status")///TODO: Remove magick key
                                    {
                                        target.Status = assertionValue;
                                    }
                                    else if (assertionKey == "Player") ///TODO: Remove magick string
                                    {
                                        if (assertionValue == "hp.dec") ///TODO: Remove magic string
                                        {
                                            hero.hp--;
                                        }
                                    }
                                    else if (assertionKey == "Move") ///TODO: You know what to do. 
                                    {
                                        Adventure.Parser parser = new();
                                        currentLocation = parser.CreateLocationFromDescription($"game/{assertionValue}");
                                        currentDescription = $"{currentDescription}\n{currentLocation.Description}";
                                    }

                                    if (assertionKey == "Player" && assertionValue.StartsWith("Inventory.add"))
                                    {
                                        string itemName = assertionValue.Substring("Inventory.add".Length).Trim();
                                        if (currentLocation.Inventory.ContainsKey(itemName))
                                        {
                                            playerInventory.Add(currentLocation.Inventory[itemName]);
                                            currentLocation.Inventory.Remove(itemName);
                                        }
                                        else
                                        {
                                            // If the item is not in the current location's inventory, print an error message
                                            Console.WriteLine($"Error: Item '{itemName}' not found in current location's inventory.");
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            currentDescription = "That is not possible";///TODO: Remove magick string, make feadback less static?
                        }
                    }
                    else
                    {
                        currentDescription = "That does nothing";//TODO: Remove magick string, make feadback less static?
                    }
                }

                command = String.Empty;

                // This is not a good solution, rewrite for clarity.
                if (hero.hp == 0)
                {
                    currentDescription += "You died 💀"; //TODO: Remove magick string, les statick feadback?

                }



            }
        }
        public void Draw()
        {
            if (dirty)
            {
                dirty = false;
                int currentRow = startRow;
                int currentColumn = startColumn;
                Console.Clear();

                Write(ANSICodes.Positioning.SetCursorPos(currentRow, currentColumn));

                string[] lines = SplitIntoLines(currentDescription, MAX_LINE_WIDTH);
                foreach (string line in lines)
                {
                    Write(Reset(ColorizeWords(line, ANSICodes.Colors.Blue, ANSICodes.Colors.Yellow)), newLine: true);
                }
                currentRow = Console.CursorTop + PADDING;
                Write(ANSICodes.Positioning.SetCursorPos(currentRow, currentColumn));
                Write($"{new string(DASH[0], MAX_LINE_WIDTH)}", newLine: true);

                // Display the inventory
                currentRow = Console.CursorTop + PADDING;
                Write(ANSICodes.Positioning.SetCursorPos(currentRow, currentColumn));
                Write("Inventory: " + playerInventory.ToString(), newLine: true);

                currentRow = Console.CursorTop + PADDING;
                Write(ANSICodes.Positioning.SetCursorPos(currentRow, currentColumn));
                Write(PROMPT_SYMBOL + commandBuffer);
            }
        }

        #region Helper Functions ------------------------------------------------------------------------------------

        void ProcessKey()
        {
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);
            if (keyInfo.Key == ConsoleKey.Enter)
            {
                command = commandBuffer;
                commandBuffer = String.Empty;
            }
            else if (keyInfo.Key == ConsoleKey.Backspace)
            {
                if (commandBuffer.Length > 0)
                {
                    commandBuffer = commandBuffer.Substring(0, commandBuffer.Length - 1);
                }
            }
            else
            {
                commandBuffer += keyInfo.KeyChar;
            }
        }


        string[] SplitIntoLines(string str, int maxLineLength)
        {
            List<string> lines = new List<string>();
            int index = 0;
            while (index < str.Length)
            {
                int remainingLength = str.Length - index;
                int length = Math.Min(maxLineLength, remainingLength);
                string line = str.Substring(index, length);
                int leadingSpaces = (maxLineLength - line.Length) / 2;
                line = new string(' ', leadingSpaces) + line;
                lines.Add(line);
                index += line.Length;
            }
            return lines.ToArray();
        }

        #endregion


    }
}
