using FluentValidation;
using SmartMix.Core.Contracts.BaseModels;

namespace SmartMix.Core.API.Validators;

public class StartBatchRequestValidator : AbstractValidator<StartBatchRequest>
{
    public StartBatchRequestValidator()
    {
        RuleFor(x => x.ApplicationId).GreaterThan(0).WithMessage("ApplicationId must be greater than 0");
        RuleFor(x => x.MixerNumber).InclusiveBetween(1, 8).WithMessage("MixerNumber must be between 1 and 8");
    }
}

public class DoseMaterialRequestValidator : AbstractValidator<DoseMaterialRequest>
{
    public DoseMaterialRequestValidator()
    {
        RuleFor(x => x.MixerNumber).InclusiveBetween(1, 8).WithMessage("MixerNumber must be between 1 and 8");
        RuleFor(x => x.MaterialId).GreaterThan(0).WithMessage("MaterialId must be greater than 0");
    }
}

public class CompleteBatchRequestValidator : AbstractValidator<CompleteBatchRequest>
{
    public CompleteBatchRequestValidator()
    {
        RuleFor(x => x.MixerNumber).InclusiveBetween(1, 8).WithMessage("MixerNumber must be between 1 and 8");
    }
}

public class CreateRecipeRequestValidator : AbstractValidator<CreateRecipeRequest>
{
    public CreateRecipeRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100).WithMessage("Name is required and must be <= 100 chars");
        RuleFor(x => x.UserId).GreaterThan(0).WithMessage("UserId must be greater than 0");
        RuleFor(x => x.CategoryId).GreaterThan(0).WithMessage("CategoryId must be greater than 0");
        RuleFor(x => x.TimeSetId).GreaterThan(0).WithMessage("TimeSetId must be greater than 0");
        RuleFor(x => x.MixerSetId).GreaterThan(0).WithMessage("MixerSetId must be greater than 0");
        RuleFor(x => x.Structures).NotEmpty().WithMessage("At least one structure is required");
        RuleForEach(x => x.Structures).SetValidator(new CreateRecipeStructureRequestValidator());
    }
}

public class CreateRecipeStructureRequestValidator : AbstractValidator<CreateRecipeStructureRequest>
{
    public CreateRecipeStructureRequestValidator()
    {
        RuleFor(x => x.ComponentId).GreaterThan(0).WithMessage("ComponentId must be greater than 0");
        RuleFor(x => x.ComponentName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Percentage).GreaterThan(0).LessThanOrEqualTo(100).WithMessage("Percentage must be > 0 and <= 100");
        RuleFor(x => x.TargetWeightKg).GreaterThanOrEqualTo(0).WithMessage("TargetWeightKg must be >= 0");
    }
}

public class UpdateRecipeRequestValidator : AbstractValidator<UpdateRecipeRequest>
{
    public UpdateRecipeRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleForEach(x => x.Structures!).SetValidator(new CreateRecipeStructureRequestValidator());
    }
}

public class CalculateMaterialsRequestValidator : AbstractValidator<CalculateMaterialsRequest>
{
    public CalculateMaterialsRequestValidator()
    {
        RuleFor(x => x.RecipeId).GreaterThan(0);
        RuleFor(x => x.TotalVolumeM3).GreaterThan(0).WithMessage("TotalVolumeM3 must be greater than 0");
    }
}

public class SetBunkerComponentRequestValidator : AbstractValidator<SetBunkerComponentRequest>
{
    public SetBunkerComponentRequestValidator()
    {
        RuleFor(x => x.BunkerNumber).InclusiveBetween(1, 64);
        RuleFor(x => x.LineNumber).GreaterThan(0);
        RuleFor(x => x.MaterialId).GreaterThan(0);
        RuleFor(x => x.UserId).GreaterThan(0);
    }
}

public class ToggleBunkerRequestValidator : AbstractValidator<ToggleBunkerRequest>
{
    public ToggleBunkerRequestValidator()
    {
        RuleFor(x => x.BunkerNumber).InclusiveBetween(1, 64);
        RuleFor(x => x.LineNumber).GreaterThan(0);
        RuleFor(x => x.UserId).GreaterThan(0);
    }
}

public class CalibrateBunkerRequestValidator : AbstractValidator<CalibrateBunkerRequest>
{
    public CalibrateBunkerRequestValidator()
    {
        RuleFor(x => x.BunkerNumber).InclusiveBetween(1, 64);
        RuleFor(x => x.LineNumber).GreaterThan(0);
        RuleFor(x => x.ReferenceWeightKg).GreaterThan(0);
    }
}

public class ConsumptionReportRequestValidator : AbstractValidator<ConsumptionReportRequest>
{
    public ConsumptionReportRequestValidator()
    {
        RuleFor(x => x.From).LessThanOrEqualTo(x => x.To).WithMessage("From must be <= To");
        RuleFor(x => x.To).GreaterThanOrEqualTo(x => x.From);
    }
}

public class AuthenticateRequestValidator : AbstractValidator<AuthenticateRequest>
{
    public AuthenticateRequestValidator()
    {
        RuleFor(x => x.Username).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(4);
    }
}

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.CurrentPassword).NotEmpty();
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(4);
    }
}

public class ActivateLicenseRequestValidator : AbstractValidator<ActivateLicenseRequest>
{
    public ActivateLicenseRequestValidator()
    {
        RuleFor(x => x.SerialNumber).NotEmpty().MaximumLength(100);
    }
}

public class ValidateLicenseRequestValidator : AbstractValidator<ValidateLicenseRequest>
{
    public ValidateLicenseRequestValidator()
    {
        RuleFor(x => x.SerialNumber).NotEmpty().MaximumLength(100);
    }
}