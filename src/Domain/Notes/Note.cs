using neatbook.Domain.Common.Interfaces;
using neatbook.Domain.Notes.Enums;
using neatbook.Domain.Notes.Events;

namespace neatbook.Domain.Notes; 

public class Note : BaseAuditableEntity, IAggregateRoot {
  public string Title { get; private set; }
  public string Content { get; private set; }
  public NoteBackground Background { get; private set; }
  public List<NotePicture> Pictures { get; private set; } = new();
  public string OwnerId { get; private set; }
  public List<NoteCollaborator> Collaborators { get; private set; } = new();
  public List<Label> Labels { get; private set; } = new();
  public bool IsArchived { get; private set; }

  public Note(string title, string content, string ownerId, NoteBackground background = NoteBackground.Default) {
    Title = title;
    Content = content;
    Background = background;
    OwnerId = ownerId;
    IsArchived = false;
  }

  public void AddCollaborator(string collaboratorId,
    CollaboratorPermissions permissions = CollaboratorPermissions.Read) {
    if (collaboratorId == OwnerId) {
      throw new ArgumentException("Cannot add the owner as a collaborator.", nameof(collaboratorId));
    }

    // If that user is already a collaborator, just change their permissions to the new ones
    NoteCollaborator? noteCollaborator = Collaborators.FirstOrDefault(c => c.CollaboratorId == collaboratorId);
    if (noteCollaborator is not null) {
      noteCollaborator.ChangeCollaboratorPermissions(permissions);
    } else {
      Collaborators.Add(new NoteCollaborator(
        collaboratorId,
        this.Id,
        permissions
      ));
    }
  }

  public void RemoveCollaborator(string collaboratorId) {
    NoteCollaborator? noteCollaborator = Collaborators.FirstOrDefault(c => c.CollaboratorId == collaboratorId);
    if (noteCollaborator is not null) {
      Collaborators.Remove(noteCollaborator);
    }
  }

  public bool UserCanEdit(string userId) {
    if (IsArchived) {
      return false;
    }

    if (userId == OwnerId) {
      return true;
    }

    return Collaborators.FirstOrDefault(c => c.CollaboratorId == userId)
      ?.Permissions is CollaboratorPermissions.ReadWrite;
  }

  public void Modify(string title, string content) {
    Title = title;
    Content = content;
  }

  public void AddPicture() {
    //todo
    // Pictures.Add(new NotePicture(""));
  }

  public void RemovePicture() {
    //todo
  }

  public void AddLabel(string labelText) {
    if (Labels.Any(l => l.Name == labelText)) {
      return;
    }

    var label = new Label(labelText);
    Labels.Add(label);
  }

  public void RemoveLabel(string labelText) {
    Labels.RemoveAll(l => l.Name == labelText);
  }

  public void Archive() {
    if (IsArchived) {
      return;
    }

    IsArchived = true;
    AddDomainEvent(new NoteArchivedEvent(this));
  }
}
