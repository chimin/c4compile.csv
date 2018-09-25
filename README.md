# c4compile.csv

Somehow I just feel like writing a CSV library


## To read line as list

``` c#
var reader = new CsvReader(textReader);
while (!reader.EndOfInput) {
  var line = reader.ReadLine();
  // do your things here
}
```

## To read line as dictionary

``` c#
var reader = new CsvReader(textReader);
reader.ReadHeaders();
while (!reader.EndOfInput) {
  var dict = reader.ReadDictionary();
  // do your things here
}
```

## To read line as object

``` c#
var reader = new CsvReader(textReader);
reader.ReadHeaders();
while (!reader.EndOfInput) {
  var t = reader.ReadObject<T>();
  // do your things here
}
```


## To write some items

``` c#
var writer = new CsvWriter(textWriter);
writer.WriteLine("item 1", "item 2", "item 3");
```

## To write a dictionary

``` c#
var writer = new CsvWriter(textWriter);
writer.Headers = ...;
writer.WriteDictionary(...);
```

## To write an object

``` c#
var writer = new CsvWriter(textWriter);
writer.Headers = ...;
writer.WriteObject(...);
```
