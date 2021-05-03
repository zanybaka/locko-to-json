// https://json2csharp.com/json-to-csharp was used to convert JSON to c#

using System.Collections.Generic;

namespace locko_to_json.BitwardenDto
{
    public class Folder
    {
        public string id;
        public string name;
    }

    public class Field
    {
        public string name;
        public string value;
        public int type;
    }

    public class Uri
    {
        public object match;
        public string uri;
    }

    public class Login
    {
        public List<Uri> uris;
        public string username;
        public string password;
        public string totp;
        public string otpauth;
    }

    public class SecureNote
    {
        public int type;
    }

    public class Card
    {
        public string cardholderName;
        public string brand;
        public string number;
        public string expMonth;
        public string expYear;
        public string code;
    }

    public class Identity
    {
        public string title;
        public string firstName;
        public string middleName;
        public string lastName;
        public string address1;
        public object address2;
        public object address3;
        public string city;
        public string state;
        public string postalCode;
        public string country;
        public string company;
        public string email;
        public string phone;
        public string ssn;
        public string username;
        public string passportNumber;
        public string licenseNumber;
    }

    public class BitwardenItem
    {
        public string id;
        public object organizationId;
        public string folderId;
        public int type;
        public string name;
        public string notes;
        public bool favorite;
        public List<Field> fields;
        public Login login;
        public List<object> collectionIds;
        public SecureNote secureNote;
        public Card card;
        public Identity identity;
    }

    public class Bitwarden
    {
        public bool encrypted;
        public List<Folder> folders;
        public List<BitwardenItem> items;
    }
}