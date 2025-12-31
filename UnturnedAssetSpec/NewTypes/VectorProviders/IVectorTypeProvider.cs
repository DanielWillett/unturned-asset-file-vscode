using System;
using System.Diagnostics.CodeAnalysis;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// Defines operation for vectors of type <typeparamref name="TVector"/>.
/// </summary>
public interface IVectorTypeProvider<TVector>
    where TVector : IEquatable<TVector>
{
    /// <value>
    /// The number of components in this vector type.
    /// </value>
    int Size { get; }

    /// <summary>
    /// Performs a component-wise multiplication on this vector.
    /// </summary>
    /// <exception cref="NotSupportedException"/>
    TVector? Multiply(TVector? val1, TVector? val2);

    /// <summary>
    /// Performs a component-wise division on this vector.
    /// </summary>
    /// <exception cref="NotSupportedException"/>
    TVector? Divide(TVector? val1, TVector? val2);

    /// <summary>
    /// Performs a component-wise addition on this vector.
    /// </summary>
    /// <exception cref="NotSupportedException"/>
    TVector? Add(TVector? val1, TVector? val2);

    /// <summary>
    /// Performs a component-wise subtraction on this vector.
    /// </summary>
    /// <exception cref="NotSupportedException"/>
    TVector? Subtract(TVector? val1, TVector? val2);

    /// <summary>
    /// Performs a component-wise modulus on this vector.
    /// </summary>
    /// <exception cref="NotSupportedException"/>
    TVector? Modulo(TVector? val1, TVector? val2);

    /// <summary>
    /// Performs a component-wise power operation on this vector.
    /// </summary>
    /// <exception cref="NotSupportedException"/>
    TVector? Power(TVector? val1, TVector? val2);

    /// <summary>
    /// Creates a new vector with the minimum values of each component.
    /// </summary>
    /// <exception cref="NotSupportedException"/>
    TVector? Min(TVector? val1, TVector? val2);

    /// <summary>
    /// Creates a new vector with the maximum values of each component.
    /// </summary>
    /// <exception cref="NotSupportedException"/>
    TVector? Max(TVector? val1, TVector? val2);

    /// <summary>
    /// Performs a component-wise mean average opearation on this vector.
    /// </summary>
    /// <exception cref="NotSupportedException"/>
    TVector? Avg(TVector? val1, TVector? val2);

    /// <summary>
    /// Creates a new vector with the absolute values of each component.
    /// </summary>
    /// <exception cref="NotSupportedException"/>
    [return: NotNullIfNotNull(nameof(val))]
    TVector? Absolute(TVector? val);

    /// <summary>
    /// Creates a new vector with the rounded values of each component.
    /// </summary>
    /// <exception cref="NotSupportedException"/>
    [return: NotNullIfNotNull(nameof(val))]
    TVector? Round(TVector? val);

    /// <summary>
    /// Creates a new vector with the ceil'd values of each component.
    /// </summary>
    /// <exception cref="NotSupportedException"/>
    [return: NotNullIfNotNull(nameof(val))]
    TVector? Ceiling(TVector? val);

    /// <summary>
    /// Creates a new vector with the floor'd values of each component.
    /// </summary>
    /// <exception cref="NotSupportedException"/>
    [return: NotNullIfNotNull(nameof(val))]
    TVector? Floor(TVector? val);

    /// <summary>
    /// Performs a component-wise trigonometric operation on <paramref name="val"/>.
    /// </summary>
    /// <param name="deg">Whether or not to perform the operation in degrees instead of radians.</param>
    /// <param name="op">
    /// Which trigonometric function to perform.
    /// <list type="none">
    ///     <item>0. sine</item>
    ///     <item>1. cosine</item>
    ///     <item>2. tangent</item>
    ///     <item>3. arcsine</item>
    ///     <item>4. arccosine</item>
    ///     <item>5. arctangent</item>
    /// </list>
    /// </param>
    /// <exception cref="NotSupportedException"/>
    [return: NotNullIfNotNull(nameof(val))]
    TVector? TrigOperation(TVector? val, int op, bool deg);

    /// <summary>
    /// Performs a component-wise inverse tangent operation on <paramref name="x"/>.
    /// </summary>
    /// <param name="deg">Whether or not to perform the operation in degrees instead of radians.</param>
    [return: NotNullIfNotNull(nameof(x))]
    TVector? Atan2(TVector? x, double y, bool deg);

    /// <summary>
    /// Performs a component-wise inverse tangent operation on <paramref name="y"/>.
    /// </summary>
    /// <param name="deg">Whether or not to perform the operation in degrees instead of radians.</param>
    [return: NotNullIfNotNull(nameof(y))]
    TVector? Atan2(double x, TVector? y, bool deg);

    /// <summary>
    /// Performs a component-wise inverse tangent operation on <paramref name="x"/> and <paramref name="y"/>.
    /// </summary>
    /// <param name="deg">Whether or not to perform the operation in degrees instead of radians.</param>
    TVector? Atan2(TVector? x, TVector? y, bool deg);

    /// <summary>
    /// Performs a component-wise square root opearation on this vector.
    /// </summary>
    /// <exception cref="NotSupportedException"/>
    [return: NotNullIfNotNull(nameof(val))]
    TVector? Sqrt(TVector? val);

    /// <summary>
    /// Creates a vector where all components are equal to <paramref name="scalar"/>.
    /// </summary>
    /// <remarks>Depending on the type of vector the components may be rounded or clamped to fit the bounds of the vector.</remarks>
    /// <exception cref="NotSupportedException"/>
    TVector Construct(double scalar);

    /// <summary>
    /// Creates a vector with the given components. Extra components will be ignored and missing components will be initialized to the default value.
    /// </summary>
    /// <remarks>Depending on the type of vector the components may be rounded or clamped to fit the bounds of the vector.</remarks>
    /// <exception cref="NotSupportedException"/>
    TVector Construct(ReadOnlySpan<double> components);

    /// <summary>
    /// Extracts the components from a vector. Extra or missing space will be ignored.
    /// </summary>
    /// <exception cref="NotSupportedException"/>
    int Deconstruct(TVector val, Span<double> components);

    /// <summary>
    /// Compares the components of one vector to another, starting from the first component and moving down the line as each component is equal.
    /// </summary>
    int Compare(TVector left, TVector right);

    /// <summary>
    /// Gets the component at a given zero-based index.
    /// </summary>
    /// <returns>The value of the given component, or 0 if the component index is out of bounds.</returns>
    /// <exception cref="NotSupportedException"/>
    double GetComponent(TVector val, int index);

    /// <summary>
    /// Converts this vector to a string value.
    /// </summary>
    [return: NotNullIfNotNull(nameof(val))]
    string? ToString(TVector? val);

    /// <summary>
    /// Attempt to parse a vector from a string value.
    /// </summary>
    bool TryParse([NotNullWhen(true)] string? str, [MaybeNullWhen(false)] out TVector vector);
}