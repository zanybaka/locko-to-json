using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using locko_to_json.BitwardenDto;
using Newtonsoft.Json;
using ErrorEventArgs = Newtonsoft.Json.Serialization.ErrorEventArgs;
using Uri = locko_to_json.BitwardenDto.Uri;

namespace locko_to_json
{
    internal class Program
    {
        private static void Main()
        {
            string[] files = Directory.GetFiles(".", "*.item");
            if (files?.Length == 0)
            {
                Console.WriteLine("No Locko items found. Please export your data from Locko and extract the zip file into the current folder.");
                Environment.Exit(exitCode: 0);
            }

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Error                 += OnError;
            settings.MissingMemberHandling =  MissingMemberHandling.Error;

            Bitwarden bitwarden = new Bitwarden
            {
                folders = new List<Folder>(),
                items   = new List<BitwardenItem>()
            };
            Folder importedFromLockoFolder = new Folder
            {
                id   = Guid.NewGuid().ToString(),
                name = "Imported from Locko"
            };
            bitwarden.folders.Add(importedFromLockoFolder);
            string[] fieldNames = typeof(Fields).GetFields(BindingFlags.Instance | BindingFlags.Public).Select(x => x.Name).ToArray();
            foreach (string file in files)
            {
                LockoItem lockoItem = JsonConvert.DeserializeObject<LockoItem>(File.ReadAllText(file), settings);
                string[]  subFiles  = Directory.GetFiles(lockoItem.uuid, "*.item");
                foreach (string subFile in subFiles)
                {
                    LockoItem     lockoSubItem  = JsonConvert.DeserializeObject<LockoItem>(File.ReadAllText(subFile), settings);
                    BitwardenItem bitwardenItem = new BitwardenItem();
                    bitwardenItem.id       = Guid.NewGuid().ToString();
                    bitwardenItem.folderId = importedFromLockoFolder.id;
                    bitwardenItem.name     = lockoSubItem.title;
                    bitwardenItem.notes    = lockoSubItem.data.fields.note;
                    bitwardenItem.fields = fieldNames
                        .Select(x => BitwardenHelper.ConvertToBitwardenField(
                                    x,
                                    typeof(Fields).GetField(x, BindingFlags.Instance | BindingFlags.Public)
                                        .GetValue(lockoSubItem.data.fields) as string))
                        .Where(x => x.value != null)
                        .ToList();
                    // TODO: read password history
                    // TODO: read form fields
                    switch (lockoItem.title)
                    {
                        case "Wallet":
                            bitwardenItem.card = new Card();
                            bitwardenItem.type = 3; // card
                            if (lockoSubItem.data.fields.type?.ToLowerInvariant().Contains("mastercard") == true)
                                bitwardenItem.card.brand = "MasterCard";
                            else if (lockoSubItem.data.fields.type?.ToLowerInvariant().Contains("visa") == true) bitwardenItem.card.brand = "Visa";
                            bitwardenItem.card.cardholderName = lockoSubItem.data.fields.name;
                            bitwardenItem.card.code           = lockoSubItem.data.fields.cardSecurityCode;
                            if (lockoSubItem.data.fields.expiryDate > 0)
                            {
                                DateTime expiryTime = new DateTime(year: 2001, month: 1, day: 1, hour: 0, minute: 0, second: 0, DateTimeKind.Utc)
                                    .ToLocalTime().Add(TimeSpan.FromSeconds(lockoSubItem.data.fields.expiryDate));
                                bitwardenItem.card.expMonth = expiryTime.Month.ToString();
                                bitwardenItem.card.expYear  = expiryTime.Year.ToString();
                            }
                            bitwardenItem.card.number   = lockoSubItem.data.fields.cardNumber;
                            break;
                        case "Web Logins":
                            bitwardenItem.login          = new Login();
                            bitwardenItem.type           = 1; // login
                            bitwardenItem.login.username = lockoSubItem.data.fields.username;
                            bitwardenItem.login.password = lockoSubItem.data.fields.password;
                            bitwardenItem.login.uris     = new List<Uri>();
                            Uri uri = new Uri();
                            uri.uri = lockoSubItem.data.fields.url;
                            bitwardenItem.login.uris.Add(uri);
                            break;
                        case "Other":
                        case "Notes":
                            bitwardenItem.secureNote = new SecureNote();
                            bitwardenItem.type       = 2; // note
                            break;
                        default:
                            throw new NotSupportedException($"Lock item '{lockoItem.title}' is not supported yet.");
                    }

                    bitwarden.items.Add(bitwardenItem);
                }
            }

            Console.WriteLine($"Read {bitwarden.items.Count} items.");

            File.WriteAllText("Bitwarden.json", JsonConvert.SerializeObject(bitwarden, Formatting.Indented).Replace("\r\n", "\n"));
            Console.WriteLine("Done. Saved to Bitwarden.json");
            Console.ReadKey();
        }

        private static void OnError(object? sender, ErrorEventArgs e)
        {
            Console.WriteLine(e.ErrorContext.Error);
        }
    }
}