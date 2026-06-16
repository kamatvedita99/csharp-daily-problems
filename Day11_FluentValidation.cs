/*
 * Day 11 — FluentValidation
 *
 * Problem:
 * Write validators for CreateVendorRequest and CreateSymbologyRequest.
 * Understand how FluentValidation replaces data annotation attributes.
 */

using FluentValidation;

public class CreateVendorRequest
{
    public string Name { get; set; } = string.Empty;
    public string ShortCode { get; set; } = string.Empty;
}

public class CreateSymbologyRequest
{
    public string TypeCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class CreateVendorRequestValidator : AbstractValidator<CreateVendorRequest>
{
    public CreateVendorRequestValidator()
    {
        RuleFor(cvr => cvr.Name)
            .NotEmpty()
            .MaximumLength(100)
            .Matches(@"^[^0-9]*$")
            .WithMessage("Name cannot contain numbers.");

        RuleFor(cvr => cvr.ShortCode)
            .NotEmpty()
            .MaximumLength(20)
            .Matches(@"^[A-Z]+$")
            .WithMessage("ShortCode must be uppercase letters only, no spaces.");
    }
}

public class CreateSymbologyRequestValidator : AbstractValidator<CreateSymbologyRequest>
{
    public CreateSymbologyRequestValidator()
    {
        RuleFor(csr => csr.TypeCode)
            .NotEmpty()
            .MaximumLength(20)
            .Matches(@"^[A-Z]+$")
            .WithMessage("TypeCode must be uppercase letters only, no spaces.");

        RuleFor(csr => csr.Description)
            .NotEmpty()
            .MaximumLength(250);
    }
}

/*
 * ─────────────────────────────────────────────
 * FluentValidation — what, why, when
 * ─────────────────────────────────────────────
 *
 * WHAT is FluentValidation?
 * A library for building strongly-typed validation rules in a fluent chain.
 * Alternative to Data Annotations ([Required], [MaxLength]) on model properties.
 *
 * WHY use FluentValidation over Data Annotations?
 *
 * 1. Separation of concerns
 *    Data Annotations mix validation logic into your model class.
 *    FluentValidation keeps validation in its own class — single responsibility.
 *
 * 2. More powerful rules
 *    Data Annotations are limited. FluentValidation has:
 *    - Regex matching
 *    - Cross-property validation (e.g. EndDate > StartDate)
 *    - Conditional rules (.When())
 *    - Custom validators (.Must())
 *    - Async validators (.MustAsync())
 *
 * 3. Testable
 *    Validators are plain classes — easy to unit test in isolation.
 *    var validator = new CreateVendorRequestValidator();
 *    var result = validator.Validate(request);
 *    Assert.True(result.IsValid);
 *
 * 4. Better error messages
 *    .WithMessage() gives you full control over error text.
 *    Data Annotations have rigid message formats.
 *
 * ─────────────────────────────────────────────
 * Key built-in validators
 * ─────────────────────────────────────────────
 *
 * .NotEmpty()          → not null, not empty string, not whitespace
 * .NotNull()           → not null only (empty string passes)
 * .MaximumLength(n)    → string max length
 * .MinimumLength(n)    → string min length
 * .Length(min, max)    → string length range
 * .Matches(regex)      → regex pattern match
 * .EmailAddress()      → valid email format
 * .GreaterThan(n)      → numeric greater than
 * .LessThan(n)         → numeric less than
 * .InclusiveBetween()  → numeric range inclusive
 * .Must(func)          → custom predicate
 * .MustAsync(func)     → async custom predicate
 * .WithMessage(msg)    → custom error message for previous rule
 * .When(condition)     → apply rule only when condition is true
 *
 * ─────────────────────────────────────────────
 * Chaining rules on same property
 * ─────────────────────────────────────────────
 *
 * Each rule in a chain is independent — all are evaluated.
 * If NotEmpty fails, MaximumLength still runs.
 * Use .WithMessage() after the specific rule it belongs to.
 *
 * RuleFor(x => x.Name)
 *     .NotEmpty().WithMessage("Name is required.")
 *     .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.")
 *     .Matches(@"^[^0-9]*$").WithMessage("Name cannot contain numbers.");
 *
 * ─────────────────────────────────────────────
 * Wiring up in ASP.NET Core
 * ─────────────────────────────────────────────
 *
 * Install:
 *   FluentValidation
 *   FluentValidation.DependencyInjectionExtensions
 *
 * In Program.cs:
 *   builder.Services.AddValidatorsFromAssemblyContaining<CreateVendorRequestValidator>();
 *
 * This registers all validators in the assembly automatically.
 *
 * In controller — inject and use:
 *   public async Task<IActionResult> Create(
 *       CreateVendorRequest request,
 *       IValidator<CreateVendorRequest> validator)
 *   {
 *       var result = await validator.ValidateAsync(request);
 *       if (!result.IsValid)
 *           return BadRequest(result.Errors);
 *   }
 *
 * OR use automatic validation via middleware — validates before controller is hit.
 *
 * ─────────────────────────────────────────────
 * Common regex patterns used in financial APIs
 * ─────────────────────────────────────────────
 *
 * Uppercase only, no spaces:    ^[A-Z]+$
 * Uppercase + numbers:          ^[A-Z0-9]+$
 * No numbers:                   ^[^0-9]*$
 * ISIN format (2 letters + 10 alphanumeric): ^[A-Z]{2}[A-Z0-9]{10}$
 * Currency code (3 uppercase):  ^[A-Z]{3}$
 * Country code (2 uppercase):   ^[A-Z]{2}$
 */
