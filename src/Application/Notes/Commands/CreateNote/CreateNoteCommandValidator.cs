namespace neatbook.Application.Notes.Commands.CreateNote; 

public class CreateNoteCommandValidator : AbstractValidator<CreateNoteCommand> {
  public CreateNoteCommandValidator() {
    RuleFor(v => v.Title)
      .MaximumLength(200)
      .NotEmpty();
    RuleFor(v => v.Content)
      .NotEmpty();
    RuleFor(v => v.UserId)
      .NotEmpty();
  }
}
