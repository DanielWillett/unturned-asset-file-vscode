# Expressions

Expressions allow calculating values based on other values. Usually this is used for the [DefaultValue](./default-values.md) property.


## Functions

Operators (like `+`, `-`, etc) are <b>NOT</b> supported, all math has to be done using functions.

The following functions are available:

| Function | Description                              | Signature                        |
| -------- | ---------------------------------------- | -------------------------------- |
| PI       | Value of PI                              | `PI -> number`                   |
| TAU      | Value of PI * 2                          | `TAU -> number`                  |
| E        | Value of E (euler's number)              | `E -> number`                    |
| NULL     | Null value                               | `NULL -> null`                   |
| ABS      | Absolute Value                           | `ABS(number) -> number`          |
| ROUND    | Round to nearest integer                 | `ROUND(number) -> integer`       |
| FLOOR    | Round to closest integer &lt;= value     | `FLOOR(number) -> integer`       |
| CEIL     | Round to closest integer &gt;= value     | `CEIL(number) -> integer`        |
| SINR     | Sine (radians)                           | `SINR(number) -> number`         |
| SIND     | Sine (degrees)                           | `SIND(number) -> number`         |
| COSR     | Cosine (radians)                         | `COSR(number) -> number`         |
| COSD     | Cosine (degrees)                         | `COSD(number) -> number`         |
| TANR     | Tangent (radians)                        | `TANR(number) -> number`         |
| TAND     | Tangent (degrees)                        | `TAND(number) -> number`         |
| ASINR    | Arcsine (radians)                        | `ASINR(number) -> number`        |
| ASIND    | Arcsine (degrees)                        | `ASIND(number) -> number`        |
| ACOSR    | Arccosine (radians)                      | `ACOSR(number) -> number`        |
| ACOSD    | Arccosine (degrees)                      | `ACOSD(number) -> number`        |
| ATANR    | Arctangent (radians)                     | `ATANR(number) -> number`        |
| ATAND    | Arctangent (degrees)                     | `ATAND(number) -> number`        |
| SQRT     | Square root                              | `SQRT(number) -> number`         |
| ADD      | Add                                      | `ADD(number number) -> number`   |
| SUB      | Subtract                                 | `SUB(number number) -> number`   |
| MUL      | Multiply                                 | `MUL(number number) -> number`   |
| DIV      | Divide                                   | `DIV(number number) -> number`   |
| MOD      | Modulo (remainder)                       | `MOD(number number) -> number`   |
| MIN      | Minimum                                  | `MIN(number number) -> number`   |
| MAX      | Maximum                                  | `MAX(number number) -> number`   |
| AVG      | Average (mean)                           | `AVG(number number) -> number`   |
| POW      | Exponent (arg1 ^ arg2)                   | `POW(number number) -> number`   |
| CAT      | Concatenate (combine two strings)        | `CAT(any any) -> string`         |
| REP      | Replace (all "arg2" -> "arg3" in "arg1") | `REP(any any any) -> string`     |

## Format
All expressions start with an equal sign, then are followed with a function name.

After a function, arguments are specified with parenthesis. Arguments are input values for functions. Functions with no arguments should not specify parenthesis.

Function arguments are separated by a normal space (` `). Do not use other whitespace characters and do not add extra spaces anywhere.

```json
// 1.0
// Math.Abs(-1);
"=ABS(-1)"
```

```json
// 3.14159...
// Math.PI;
"=PI"
```

Function arguments can be [Dynamic Values](./dynamic-values.md).

```json
// 0.1 or whatever Bullet_Drop is set to, whichever is higher
// Math.Max(this.Bullet_Drop, 0.1);
"=MAX(@Bullet_Drop 0.1)",

// 0
// Math.Min(Math.Min(0, 2), 4);
"=MIN(=MIN(0 2) 4)"
```

Arguments which require spaces can be surrounded in parenthesis. There is currently no way to escape parenthesis.
```json
// "3 - 6"
// string.Concat("3", " - ", "6");
"=CAT(3 =CAT(( - ) 6))"
```
```json
// "Test_Name"
// "Test Name".Replace(" ", "_");
"=REP((Test Name) ( ) _)"
```

### Invalid examples
> [!CAUTION]
> The following examples are invalid expressions.
```json
// extra spaces
"= MAX ( 3 5 )"

// comma instead of space as separator
"=ADD(1, 3)"

// missing '=' for SUB
"=ADD(SUB(1 3) 3)"

// MAX only takes 2 arguments, try nesting another MAX
"=MAX(1 2 3)"

// Functions with no arguments should not have parenthesis
"=PI()"
```