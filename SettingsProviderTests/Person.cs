using System;

namespace SettingsProviderTests
{
    /// <summary>
    /// A dummy class used for testing purposes.
    /// </summary>
    [Serializable]
    public class Person
    {
        public Person()
        {
            Name = "<noname>";
            LastName = "<noname>";
            Age = 0;
        }

        public Person(string name, string lastName, int age)
        {
            Name = name;
            LastName = lastName;
            Age = age;
        }

        public string Name { get; set; }

        public string LastName { get; set; }

        public int Age { get; set; }
    }
}
