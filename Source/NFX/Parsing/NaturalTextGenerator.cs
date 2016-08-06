/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2014 IT Adapter Inc / 2015 Aum Code LLC
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFX.Parsing
{
  /// <summary>
  /// Generates human-readable English text for tests
  /// </summary>
  public static class NaturalTextGenerator
  {

    public static readonly string[] US_STATES = new string[]
    {
       "AL", "AK", "AZ", "AR", "CA",
       "CO", "CT", "DE", "DC", "FL",
       "GA", "HI", "ID", "IL", "IN",
       "IA", "KS", "KY", "LA", "ME",
       "MD", "MA", "MI", "MN", "MS",
       "MO", "MT", "NE", "NV", "NH",
       "NJ", "NM", "NY", "NC", "ND",
       "OH", "OK", "OR", "PA", "RI",
       "SC", "SD", "TN", "TX", "UT",
       "VT", "VA", "WA", "WV", "WI",
       "WY"
    };

    public static readonly string[] DOMAIN_NAMES = new string[]
    {
       "google.com", "bing.com", "yahoo.com", "facebook.com", "twitter.com", "github.com", "nfx.io",
       "orbitz.com", "newegg.com", "ebay.com", "amazon.com", "weather.com", "youtube.com", "wikipedia.org",
       "google.co.in", "live.com", "yandex.ru", "linkedin.com", "vk.com", "reddit.com", "msn.com", "wordpress.com",
       "mail.ru", "pinterest.com", "alibaba.com", "google.es", "google.ca", "crailgslist.org", "ok.ru", "google.ru",
       "livejournal.com", "rambler.ru", "lenta.ru", "dropbox.com"
    };

    public static readonly string[] TLD_NAMES = new string[]
    {
       ".com", ".org", ".net", ".edu", ".gov", ".co", ".us", ".mobi", ".me", ".com", ".ru", ".io", ".ca",
       ".am", ".ar", ".au", ".be", ".com", ".by", ".ca", ".com", ".cs", ".de", ".dk", ".es", ".eu", ".fi", ".fr",
       ".com", ".net", ".us", ".org", ".net",
       ".gr", ".il", ".it", ".jp", ".kr", ".me", ".lv", ".md", ".com", ".no", ".pl", ".tr", ".ua", ".uk",
       ".com", ".net", ".us", ".org", ".ru", ".in", ".co.in"
    };


    public static readonly string[] POPULAR_FIRST_NAMES = new string[]
    {
      "Adam", "Alex", "Sasha", "Alexander", "Noah", "Liam", "Jacob", "Mason", "William", "Ethan", "Michael", "Jayden", "Daniel",
      "Bill", "Mike", "Bob", "Frank", "Ferdinand", "Trevor", "Dmitry", "Ivan", "Nicola", "Yasha", "Mark", "Geoff", "Jeff",
      "John", "Jon", "Ian", "Ilan", "David", "Robert", "Chuck", "Donald", "Richard", "Serge", "Victor", "Akhmed", "Amid", "Alec",
      "Dodik", "Nick", "Freddie", "Fred", "Alfred", "Harry", "Jerry", "Jacob", "Gabriel", "Logan", "James", "Jim", "Anthony",
      "Harish", "Sandeep", "Jose", "Hasan", "Hosen", "Samir", "Sumir", "Valery", "Izya", "Moshe", "Abraham", "Rajneesh",
      "Eric", "Derek", "Gary", "Benjamin", "Samuel", "Ryan", "Vincent", "Sean", "Shawn", "Stepan", "Oleg", "Vladimir", "Tolik",
      "Eugene", "Jeremy", "Don", "Ben", "Farukh", "Garik", "Dima", "Denys", "Misha", "Roma", "Roman", "Osyk", "Alik", "Maksym",
      "Max", "Tobias", "Boris", "Charles", "Edward", "Kevin", "Andrew", "Dennis", "Joshua", "Stephen", "Steve", "Larry", "Jesse",
      "Danny", "Luis", "Antonio", "Anua", "Dale", "Curtis", "Tony", "Stanley", "Manuel", "Leonard", "Randy", "Carlos", "Russell",
      "Ruben", "Pedro", "Raul", "Leslie", "Cecil", "Chester", "Andre", "Herman", "Vernon", "Ramon", "John", "Maurice", "Dustin",


      "Adelia", "Sophia", "Emma", "Olivia", "Isabella", "Rita", "Rosa", "Raya", "Inga", "Ava", "Mila", "Katya", "Helena", "Lina",
      "Dina", "Dorah", "Debbi", "Elly", "Emily", "Tonya", "Tanya", "Elizabeth", "Nastya", "Helga", "Lynda",
      "Mona", "Ornella", "Antonina", "Lisa", "Ivana", "Odessa", "Mercedes", "Lida", "Nadya", "Gabriella",
      "Linda", "Matilda", "Anthonia", "Betty", "Barbara", "Dorothy", "Sukha", "Feride", "Halida", "Karen",
      "Margaret", "Nancy", "Sarah", "Ann", "Jennifer", "Laura", "Suma", "Sharon", "Otilia", "Patricia", "Crystal",
      "Morra", "Diana", "Theresa", "Sara", "Judy", "Grace", "Janice", "Gloria", "Jacqueline", "Donna", "Angela", "Klava",
      "Nina", "Ella", "Alla", "Evgeniya", "Lena", "Luda", "Ludmila", "Beba", "Basya", "Dinara", "Gulnara", "Stella", "Lara",
      "Larissa", "Irina", "Natasha", "Helen", "Sandra", "Ruth", "Marie", "Diane", "Alice", "Julie", "Amy", "Stephanie", "Rebecca",
      "Carmen", "Hazel", "Amber", "Eva", "April", "Leslie", "Monica", "Ava", "June", "Sally", "Erica", "Dolores", "Lynn", "Alicia",
      "Jill", "Erin", "Marina", "Veronica", "Katie", "Alma", "Sue", "Beth", "Jeanne", "Pearl", "Maureen", "Colleen", "Joy", "Claudia"

    };

    public static readonly string[] POPULAR_LAST_NAMES = new string[]
    {
      "Smith", "Johnson", "Jackson", "Harris", "Brown", "Jones", "Davis", "Garcia", "Menendez", "Williams", "Miller",
      "Wilson", "Moore", "Taylor", "White", "Black", "Swift", "Martinez", "Anderson", "Lewis", "Fox", "Lee", "Scott", "Brown",
      "Adams", "Stewart", "Morris", "Sanchez", "Baker", "King", "Knight", "Cooper", "Collins", "Murphy", "Reed","Brown",
      "Cook", "Morgan", "Harris", "Williams", "Rogers", "Evans", "Parker", "Ivanov", "Petrov", "Sidorov", "Ross", "Ford", "Hamilton",
      "Fisher", "Goldman", "Harris", "Goldberg", "Silberman", "Smith", "Silverman", "Kennedy", "Twain", "Simpson", "Gagarin", "Gomez","Williams",
      "Warren", "Jonson", "Fox", "Cox", "Riley", "Cunnigham", "Heart", "Andrews", "Spencer", "Palmer", "Noel", "Edwards", "Williams",
      "Bell", "Richardson", "Smith", "Perry", "Miller", "Knob", "Cube", "Long", "Alton", "Aberman", "Doberman", "Zoe", "Zoo", "Vakyadi",
      "Chervonolobskiy", "Williams", "Jonson", "Johnson", "Dovidoff", "Semerenko", "Shilo", "Kravchenko", "Terasini", "Echt", "Bonini", "Fortaleza",
      "Odessa", "Varshavskiy", "Yablonovski", "Zigelboim", "Shreddinger", "Shreder", "Fat", "Man", "Camastra", "Chelentano",
      "More", "Mutti", "Lee", "Miller", "Bach", "Calmar", "Price", "Dassin", "Dalida", "Sinatra", "Mercury", "Kobzon", "Yasenin", "Shevchenko",
      "Serov", "Repin", "Chopin", "Mozart", "Corleone", "Smith", "Johnson", "Jonson", "Jensen", "Brown", "Popoff", "Popov", "Morgan", "Kelly",
      "Allen", "Cooper", "Moore", "Davis", "Jones", "Smith", "Brown", "Young", "White", "Lee", "Lewis", "Thomas", "Adams", "Smith", "Johnson", "Williams",
      "Neuman", "Einstein", "Borman", "Beck", "Dancer", "Dog", "Lancer", "Prayer", "Goodman", "Hopewell", "McCloud", "McKnight", "Meduza", "Mendoza",
      "Sikorski", "Petroff", "Vivaldi", "Donahue", "Grant", "Grand", "Belosi", "Pelozi", "Zimmer", "Zomer", "Youth", "Truth", "Snakes", "Drake",
      "Von Brown", "Von Neumann", "Van Gog", "Van Degraph", "D'Artatgnian", "Aramis", "Monet", "Solos", "Solon", "Euclid", "Lomonosov", "Plato", "Platon",
      "Buch", "Bush", "Groover", "Hoover", "Lincoln", "Toad", "Malcolm", "Scriabin", "Babel", "Glenn", "Leonov", "Monroe", "Moonlite", "Lite", "Liteman", "Bloomberg",
      "Bloome", "Pub", "Shrub", "Broad", "Tief", "Kartofel", "Fleisch", "Shmotzer", "Smutsich", "Sieg", "Mag", "Nasser", "Nasarudi", "Babadjanyan", "Khachaturyan",
      "Shostakovich", "Glinka", "Musorgsky", "Prokofiev", "Khazanov", "Catani", "Placido", "Leone", "Bulioni", "D'Armoniac", "D'Trevil", "De-Richelieu",
      "Bordeaux", "Mazarin", "Nazarin", "Nazareth", "Goliath", "Pupperman", "Khibibidze", "Viceman", "Wiseman", "Olivie", "Allard", "Baudini", "Adam",
      "Beaulieu", "Caron", "De la Fontaine", "Fay", "Abandonato", "Genis", "Gedis", "Adamaitis", "Accardi", "Anua", "Agnelutti", "Tutti", "Maestoso",
      "Fuocco", "De Vitis", "De Luca", "De Rege", "De Palma", "Castro", "Cuoco", "Crespo", "Giacomo", "Colombo", "Barad", "Char", "Bassi", "Dada",
      "Bhalla", "Comar", "Tartik", "Karduk", "Tobriz", "Bhattacharyya", "Khurana", "Manda", "Goswami", "Krishnamurthy", "Swaminathan", "Venkatesh",
      "Surendrakrishnan", "Nebaran", "Moldavan", "Chaimovitsch", "Gutenmacher", "Geshefter", "Kutz", "Allyev", "Zarov", "Afonin", "Hera", "Ira", "Filipov",
      "Smith", "Brown", "Jackson", "Johnson", "Adams", "Davis", "Palmer", "Williams", "Miller", "Kozlov", "Fyodorov", "Maximova", "Orloff", "Orlov",
      "Yakovlev", "Perry", "Miller", "Heart", "Goodman", "Goodwill", "Holyman", "Long", "Fox", "Smith", "Williams", "Brown", "Romanov", "McLoud"
    };

    public static readonly string[] CITY_SUFFIXES = new string[]
    {
      "berg", "burg", "ville", "will", " Village", "dorf", " Dorf", "grad", "stan", "ovo", " Park",
      "town", " Town", "cker", " Ridge", " Heights", " Valley", " Hall", " Hill", "mento", " Bay",
      "wood", " Forest", " Woods", " Creek", " College", " Falls", " Pines", " Hills", "lin", "lyn", "ton",
      " Port", " Lake", "field", " Field", "view", " View", " Pond", " Ranch", " Beach",
      " Palace", " Lodge", " Gorge", " Glen", " Barney", " Cove", " La Mar", " Au Prince",
      " Moon Bay", " of Prussia", " Lake City", " Gul Nest", " Gul Cove"," Air", " Mount", " Harbor",
      " Coast", " Heaven", " Den", "slav", "ino", "tino", " Point", " Grove", " Grove City", " Valley View",
      " Harbor View", " Hills City", " Station"
    };

    public static readonly string[] STREET_SUFFIXES = new string[]
    {
      " way", " Way", " blvrd.", " Boulevard", " ave", " ave.", " Avenue", " avenue", " wy.", " rd",
      " Court", " court", " Way", " trail", " Trail", " rd.", " road", " Road", " Circle"
    };


    public static readonly string[] POPULAR_CITY_NAMES = new string[]
    {
      "New York", "Washington", "Philadelphia", "Cleveland", "Hudson", "Canoga Park", "Los Angeles", "Chester",
      "Bristol", "Moscow", "Odessa", "Kiev", "Marion", "Boise", "Franklin", "Greenville", "Mayfield", "Clinton",
      "Sodom", "Salem", "Fairview", "Madison", "Arnold", "Stow", "Dallas", "Ashland", "London", "Paris", "Oxford",
      "Oxnard", "Arlington", "Jackson", "Burlington", "Toronto", "Manchester", "Chita", "Milton", "Centerville",
      "Clayton", "Dayton", "Troy", "Lexington", "Milford", "Dover", "Newport", "Samara", "Bugaz", "Haifa", "Hopewell",
      "Phoenix", "San Diego", "Austin", "Indianapolis", "Athens", "Angola", "San Antonio", "Nordonia", "Macedinia",
      "North Royalton", "Seattle", "Denver", "Boston", "Nashville", "Brighton", "Albuquerque", "Omaha", "Oakland",
      "Raleigh", "Belgorod", "Belgrade", "Sofia", "Sevastopol", "Khersones", "Miami", "Atlanta", "Baltimore",
      "Tucson", "Newark", "Henderson", "Lincoln", "Greensboro", "Toledo", "Winston–Salem", "Chesapeake", "Richmond",
      "Chattanooga", "Choochoo"
    };


    public static readonly string[] LAST_NAME_SUFFIXES = new string[]
    {
      "ani", "any", "ana", "anha", "an", "avich", "asta", "astra", "astro", "ala", "ada", "adi", "acht", "ams",
      "by", "bo", "bi", "bin", "bina", "ban", "ben", "bore", "beck", "back", "backer", "burger", "berg", "bala", "butler", "brook", "bridge", "boim", "boym",
      "cle","cli","chevy", "ckin", "ckino", "ckina", "cori", "ckori", "cory", "ckory", "chuk", "chook", "choo", "chu", "chuh", "chi", "chev", "cheva",
      "dze", "dom", "dara", "dori", "dero", "der", "dorah",
      "elsk", "er", "et", "eta", "ema", "etski", "etsky", "ech", "echt", "enser",
      "fi", "fy", "fina", "fyna", "fe", "field", "feld",
      "gomery", "grey", "gray", "grim", "grihm", "gay", "gai", "gi", "gy", "go", "ga", "ge",
      "hi", "hy",
      "ini", "iny", "in", "ina", "im", "itz", "its", "itsik", "itsyk",
      "jer", "jo", "joo", "jen",
      "kin", "kino", "kina", "komo", "kori", "kari", "khmali", "khmer",
      "lar", "ler", "logo", "led", "lida", "lidah","lume","luma","ledge", "lidge",
      "man", "mas", "mana", "mark", "menko", "menka", "medov", "medova", "metov", "metova", "mali", "malykh",
      "nog", "nam", "ni", "nay", "nenko", "nenka", "nov", "nova", "nero", "nemo",
      "ovich", "ovytch", "ov", "ova", "off", "offa", "overy", "owery", "omery", "omely", "orra", "ora", "orah", "on",
      "po", "pa", "pu", "per","puma",
      "quen", "queen", "qwin",
      "rin", "rina", "rima", "riy", "riya", "ridge", "rosen",
      "smith", "ser", "shvili", "shvily", "shkin", "shkina", "soh", "sa", "so", "shevitsch", "schevich", "solo", "sanov", "sanova","sanoff", "sanoffa", "spen", "spenser",
      "tov", "tova", "tkin", "tkina", "tkyn", "tkyna", "timer", "tymer", "tzeh", "tze", "ter",
      "xi", "xoo",
      "yn", "yelsk", "yan", "yana", "yala",
      "vi", "vy", "vir", "vira",
      "war", "wer", "wor", "work", "werk", "wis",
      "ung", "undig", "una", "uno", "uni", "uma", "umah",
      "zer", "zo", "za", "zehr", "zola", "zolah", "zanov", "zanova"
    };




    /// <summary>
    /// Generates human-readable English text of up to specified length which is of 10 chars at minimum.
    /// If zero passed then generates msg of random length
    /// </summary>
    public static string Generate(int length = 150)
    {
      if (length<=0) length = 15 + NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, 135);
      if (length<10) length = 10;

      var text = getRandomTextSource();
      var i = NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, text.Length-1);
      scrollToWordStart(text, ref i);

      var result = new StringBuilder();
      var skipping = false;
      var lengthAfterLastWord = 0;
      while(true)
      {
       if (i>=text.Length)
       {
        i=0;
        scrollToWordStart(text, ref i);
       }
       var chr = text[i];
       i++;

       if (!filter(chr))
       {
         if (result.Length >= length) break;
         if (!skipping)
         {
          lengthAfterLastWord = result.Length;
         }
         skipping = true;
         continue;
       }
       if (skipping) result.Append(' ');
       skipping = false;
       result.Append( chr );
      }
      if (result.Length > length) result.Length = lengthAfterLastWord;

      return result.ToString();
    }

    /// <summary>
    /// Generates a random English word which is of the specified size. The min size must be of at least 4 chars
    /// </summary>
    public static string GenerateWord(int minLength = 4, int maxLength = 20)
    {
      if (minLength<4) minLength = 4;
      if (maxLength<minLength) maxLength = 20;

      var text = getRandomTextSource();
      var i = NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, text.Length-1);
      scrollToWordStart(text, ref i);

      var result = new StringBuilder();
      while(true)
      {
        result.Clear();

          while(true)
          {
           if (i>=text.Length)
           {
            i=0;
            scrollToWordStart(text, ref i);
           }
           var chr = text[i];
           i++;

           if (!filter(chr)) break;
           result.Append( chr );
          }

        var len = result.Length;
        if (len>=minLength && len<=maxLength) break;
      }

      return result.ToString();
    }


    /// <summary>
    /// Generates a string that resembles a human first name
    /// </summary>
    public static string GenerateFirstName()
    {
      var rnd = ExternalRandomGenerator.Instance.NextRandomInteger;

      if (rnd>-1500000000)
        return POPULAR_FIRST_NAMES[(0x7fFFFFFF & rnd) % POPULAR_FIRST_NAMES.Length];


      var  prefix = LAST_NAME_SUFFIXES[(0x7fFFFFFF & ExternalRandomGenerator.Instance.NextRandomInteger) % LAST_NAME_SUFFIXES.Length];



      var suffix = LAST_NAME_SUFFIXES[(0x7fFFFFFF & rnd) % LAST_NAME_SUFFIXES.Length];

      prefix = Char.ToUpperInvariant(prefix[0]) + prefix.Substring(1).ToLowerInvariant();
      return prefix + suffix;
    }


    /// <summary>
    /// Generates a string that resembles a human last name
    /// </summary>
    public static string GenerateLastName()
    {
      var rnd = ExternalRandomGenerator.Instance.NextRandomInteger;

      if (rnd>0 && rnd <123000000)
        return POPULAR_LAST_NAMES[(0x7fFFFFFF & rnd) % POPULAR_LAST_NAMES.Length];


      var shrt =  rnd > 1543000000;

      string prefix;

      if (shrt)
        prefix = LAST_NAME_SUFFIXES[(0x7fFFFFFF & ExternalRandomGenerator.Instance.NextRandomInteger) % LAST_NAME_SUFFIXES.Length];
      else
      {
        do
        {
          prefix = GenerateWord(4, 8);
        }
        while(prefix.IndexOf('\'')!=-1);

        prefix = prefix.ToLowerInvariant();
      }


      var suffix = LAST_NAME_SUFFIXES[(0x7fFFFFFF & rnd) % LAST_NAME_SUFFIXES.Length];

      var reverse = !shrt && rnd<-1543000000;
      if (reverse)
      {
        var t = prefix;
        prefix = suffix;
        suffix = t;
      }


      prefix = Char.ToUpperInvariant(prefix[0]) + prefix.Substring(1).ToLowerInvariant();
      return prefix + suffix;
    }


    /// <summary>
    /// Generates a string that resembles a city name
    /// </summary>
    public static string GenerateCityName()
    {
      var rnd = ExternalRandomGenerator.Instance.NextRandomInteger;

      if (rnd<-1750000000)
        return POPULAR_CITY_NAMES[(0x7fFFFFFF & rnd) % POPULAR_CITY_NAMES.Length];


      string prefix;

      if (rnd>0)
        prefix = POPULAR_FIRST_NAMES[(0x7fFFFFFF & ExternalRandomGenerator.Instance.NextRandomInteger) % POPULAR_FIRST_NAMES.Length];
      else
        prefix = POPULAR_LAST_NAMES[(0x7fFFFFFF & ExternalRandomGenerator.Instance.NextRandomInteger) % POPULAR_LAST_NAMES.Length];



      var suffix = CITY_SUFFIXES[(0x7fFFFFFF & rnd) % CITY_SUFFIXES.Length];

      prefix = Char.ToUpperInvariant(prefix[0]) + prefix.Substring(1).ToLowerInvariant();
      return prefix + suffix;
    }

    /// <summary>
    /// Generates a string that resembles a city/state/zip in a US address.
    /// The states/zips are NOT consistent
    /// </summary>
    public static string GenerateUSCityStateZip()
    {
      var state =  US_STATES[(0x7fFFFFFF & ExternalRandomGenerator.Instance.NextRandomInteger) % US_STATES.Length];
      var zip = ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, 99999);
      return "{0}, {1} {2:D5}".Args(GenerateCityName(), state, zip);
    }


    /// <summary>
    /// Generates a string that resembles an address line
    /// </summary>
    public static string GenerateAddressLine()
    {
      var streetNumber = ExternalRandomGenerator.Instance.NextScaledRandomInteger(1, 10000);

      var rnd = ExternalRandomGenerator.Instance.NextRandomInteger;

      string street;

      if (rnd % 3==0) street = GenerateFirstName();
      else
      if (rnd % 2==0) street = GenerateLastName();
      else
       street = GenerateCityName();

      if (rnd > 0)
       street +=  STREET_SUFFIXES[(0x7fFFFFFF & ExternalRandomGenerator.Instance.NextRandomInteger) % STREET_SUFFIXES.Length];

      var rnd2 =  ExternalRandomGenerator.Instance.NextRandomInteger;

      if (rnd2 % 7==0) street += " apt #"+Math.Abs(rnd2 % 18).ToString()+(char)('A'+(rnd2 & 0x8));
      else
      if (rnd2 % 5==0) street += " apt. "+Math.Abs(rnd2 % 18).ToString()+(char)('a'+(rnd2 & 0x8));
      else
      if (rnd2 % 4==0) street += " suite "+Math.Abs(rnd2 % 180).ToString();
      else
      if (rnd2 % 3==0) street += " room "+Math.Abs(rnd2 % 180).ToString();

      return "{0} {1}".Args(streetNumber, street);
    }


    /// <summary>
    /// Generates a string that looks like a human name
    /// </summary>
    public static string GenerateFullName(bool middle = false)
    {
      var fname = GenerateFirstName();
      var lname = GenerateLastName();

      if (!middle) return fname + " " + lname;

      var rnd = ExternalRandomGenerator.Instance.NextRandomInteger;

      var mname = string.Empty;
      if (rnd>0)
      {
        if ((rnd & 1) == 0)
          mname += (char)((int)'A'+(rnd % 25));
        else
          mname = GenerateFirstName();

        return fname + " " + mname + " "+ lname;
      }

      return fname + " " + lname;

    }

    /// <summary>
    /// Generates a string that looks like an email
    /// </summary>
    public static string GenerateEMail()
    {
       var rnd = ExternalRandomGenerator.Instance.NextRandomInteger;
       string domain;

       var user = GenerateFirstName();
       if (rnd%5==0) user += "."+GenerateLastName();
       if (rnd%7==0) user += (1900+Math.Abs(rnd%115)).ToString();

       if (rnd<-1500000000)
        domain = DOMAIN_NAMES[(0x7fFFFFFF & ExternalRandomGenerator.Instance.NextRandomInteger) % DOMAIN_NAMES.Length];
       else
       {
         if (rnd%3==0 && user.Length<11)
           domain = GenerateWord()+"."+GenerateCityName()+TLD_NAMES[(0x7fFFFFFF & ExternalRandomGenerator.Instance.NextRandomInteger) % TLD_NAMES.Length];
         else
           domain = GenerateWord(8)+TLD_NAMES[(0x7fFFFFFF & ExternalRandomGenerator.Instance.NextRandomInteger) % TLD_NAMES.Length];
       }

       var rnd2 = ExternalRandomGenerator.Instance.NextRandomInteger;

       if (user.Length<5 && rnd2%17!=0) user += Math.Abs(rnd%99).ToString();

       if (rnd2<1700000000) user = user.ToLowerInvariant();

       return (user+"@"+domain.ToLowerInvariant()).Replace('\'','.').Replace(' ', '.');
    }

    private static string getRandomTextSource()
    {
      return NFX.ExternalRandomGenerator.Instance.NextRandomInteger>0
        ? NFX.EmbeddedResource.GetText(typeof(NaturalTextGenerator), "JackLondon.txt")
        : NFX.EmbeddedResource.GetText(typeof(NaturalTextGenerator), "ConanDoyle.txt"); //get text does caching internally
    }


    private static void scrollToWordStart(string text, ref int i)
    {
      while(true)
      {
        var chr = text[i];
        if (chr==' ' || chr=='.' || chr==',' || chr=='?' || chr=='!' || chr=='\r' || chr=='\n') break;

        i++;
        if (i==text.Length) i=0;
      }

      while(!Char.IsLetterOrDigit(text[i]))
      {
        i++;
        if (i==text.Length) i=0;
      }
    }

    private static bool filter(char chr)
    {
      return chr=='\'' || chr=='-' || Char.IsLetterOrDigit(chr);
    }


  }
}
