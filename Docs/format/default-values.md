# Default Property Values

Default values must be specified for properties, no matter how complex their default value calculation is.

In some cases (such as below), the default value may be different if the property is included but empty or unparsable.
```cs
this.someProperty = !p.data.ContainsKey("Key") ? 1f : p.data.ParseFloat("Key");
```
If key is included but doesn't contain a value, the value will be `0`, not `1`. Many older properties follow this pattern.

Default values may also be based on other properties, which is where switches come in play.

