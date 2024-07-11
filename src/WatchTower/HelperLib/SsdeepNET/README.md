SsdeepNET
=========
A C# version of the ssdeep fuzzy text matching algorithm, http://ssdeep.sourceforge.net/.

Example
------

```cs
byte[] bytesFoo = File.ReadAllBytes("foo.txt");
byte[] bytesBar = File.ReadAllBytes("bar.txt");

FuzzyHash fuzzyHash = new();

// Returns a printable string with a maximum length of 148 characters.
string hashFoo = fuzzyHash.ComputeHash(bytesFoo);
string hashBar = fuzzyHash.ComputeHash(bytesBar);

// Returns a value from 0 to 100 indicating the match score of the two hashes.
int comparisonResult = fuzzyHash.CompareHashes(hashFoo, hashBar);

Console.WriteLine(comparisonResult);
```



Source: https://github.com/kolos450/SsdeepNET
Author: https://github.com/kolos450