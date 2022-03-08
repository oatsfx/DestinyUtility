﻿using Levante.Configs;
using Newtonsoft.Json;
using System;
using System.IO;

namespace Levante.Helpers
{
    public class ConfigHelper
    {
        public static bool CheckAndLoadConfigFiles()
        {
            BotConfig bConfig;
            DataConfig dConfig;
            ActiveConfig aConfig;

            bool closeProgram = false;

            if (!Directory.Exists("Configs")) 
                Directory.CreateDirectory("Configs");

            if (File.Exists(BotConfig.FilePath))
            {
                string json = File.ReadAllText(BotConfig.FilePath);
                bConfig = JsonConvert.DeserializeObject<BotConfig>(json);
            }
            else
            {
                bConfig = new BotConfig();
                File.WriteAllText(BotConfig.FilePath, JsonConvert.SerializeObject(bConfig, Formatting.Indented));
                Console.WriteLine($"No botConfig.json file detected. A new one has been created and the program has stopped. Go and change API tokens and other items.");
                closeProgram = true;
            }

            if (File.Exists(DataConfig.FilePath))
            {
                string json = File.ReadAllText(DataConfig.FilePath);
                dConfig = JsonConvert.DeserializeObject<DataConfig>(json);
            }
            else
            {
                dConfig = new DataConfig();
                File.WriteAllText(DataConfig.FilePath, JsonConvert.SerializeObject(dConfig, Formatting.Indented));
                Console.WriteLine($"No dataConfig.json file detected. A new one has been created and the program has stopped. No action is needed.");
                closeProgram = true;
            }

            if (File.Exists(ActiveConfig.FilePath))
            {
                string json = File.ReadAllText(ActiveConfig.FilePath);
                aConfig = JsonConvert.DeserializeObject<ActiveConfig>(json);
            }
            else
            {
                aConfig = new ActiveConfig();
                File.WriteAllText(ActiveConfig.FilePath, JsonConvert.SerializeObject(aConfig, Formatting.Indented));
                Console.WriteLine($"No activeConfig.json file detected. A new one has been created and the program has stopped. No action is needed.");
                closeProgram = true;
            }

            if (closeProgram == true) return false;
            return true;
        }
    }
}
