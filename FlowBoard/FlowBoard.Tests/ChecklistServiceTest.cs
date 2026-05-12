using NUnit.Framework;
using NSubstitute;
using FluentAssertions;
using FlowBoard.Checklist.Services;
using FlowBoard.Checklist.Models;
using FlowBoard.Checklist.Repositories;

namespace FlowBoard.Tests.Checklist
{
    [TestFixture]
    public class ChecklistServiceTests
    {
        private ILabelRepository _repo;
        private ILabelService    _sut;

        [SetUp]
        public void SetUp()
        {
            _repo = Substitute.For<ILabelRepository>();
            _sut  = new LabelServiceImpl(_repo);
        }

        // ── CreateLabel ───────────────────────────────────────────────────────

        [Test]
        public void CreateLabel_WithValidLabel_ReturnsLabel()
        {
            var label = new Label { BoardId = 1, Name = "Bug", Color = "#FF5733" };
            _repo.CreateLabel(label).Returns(label);

            var result = _sut.CreateLabel(label);

            result.Should().NotBeNull();
            result.Name.Should().Be("Bug");
            result.Color.Should().Be("#FF5733");
        }

        // ── GetLabelsByBoard ──────────────────────────────────────────────────

        [Test]
        public void GetLabelsByBoard_ReturnsAllBoardLabels()
        {
            var labels = new List<Label>
            {
                new() { LabelId = 1, BoardId = 1, Name = "Bug",     Color = "#FF5733" },
                new() { LabelId = 2, BoardId = 1, Name = "Feature",  Color = "#33FF57" }
            };
            _repo.GetLabelsByBoard(1).Returns(labels);

            var result = _sut.GetLabelsByBoard(1);

            result.Should().HaveCount(2);
            result.All(l => l.BoardId == 1).Should().BeTrue();
        }

        // ── UpdateLabel ───────────────────────────────────────────────────────

        [Test]
        public void UpdateLabel_ChangesNameAndColor()
        {
            var existing = new Label { LabelId = 1, BoardId = 1, Name = "Old", Color = "#000000" };
            var updated  = new Label { Name = "New Name", Color = "#FFFFFF" };

            _repo.GetLabelById(1).Returns(existing);
            _repo.UpdateLabel(Arg.Any<Label>()).Returns(ci => ci.Arg<Label>());

            var result = _sut.UpdateLabel(1, updated);

            result.Name.Should().Be("New Name");
            result.Color.Should().Be("#FFFFFF");
        }

        // ── DeleteLabel ───────────────────────────────────────────────────────

        [Test]
        public void DeleteLabel_CallsRepoDelete()
        {
            var label = new Label { LabelId = 1 };
            _repo.GetLabelById(1).Returns(label);

            _sut.DeleteLabel(1);

            _repo.Received(1).DeleteLabel(1);
        }

        // ── AddLabelToCard / RemoveLabelFromCard ──────────────────────────────

        [Test]
        public void AddLabelToCard_CallsRepo()
        {
            _sut.AddLabelToCard(cardId: 5, labelId: 1);

            _repo.Received(1).AddLabelToCard(5, 1);
        }

        [Test]
        public void RemoveLabelFromCard_CallsRepo()
        {
            _sut.RemoveLabelFromCard(cardId: 5, labelId: 1);

            _repo.Received(1).RemoveLabelFromCard(5, 1);
        }

        // ── CreateChecklist ───────────────────────────────────────────────────

        [Test]
        public void CreateChecklist_WithValidChecklist_ReturnsChecklist()
        {
            var checklist = new TaskChecklist { CardId = 1, Titre = "Release Checklist", Position = 1 };
            _repo.CreateChecklist(checklist).Returns(ci =>
            {
                var c = ci.Arg<TaskChecklist>();
                c.ChecklistId = 1;
                return c;
            });

            var result = _sut.CreateChecklist(checklist);

            result.Should().NotBeNull();
            result.Titre.Should().Be("Release Checklist");
            result.CardId.Should().Be(1);
        }

        // ── AddItem ───────────────────────────────────────────────────────────

        [Test]
        public void AddItem_WithValidItem_ReturnsItem()
        {
            var checklist = new TaskChecklist { ChecklistId = 1 };
            var item      = new ChecklistItem { Text = "Write unit tests", IsCompleted = false };

            _repo.GetChecklistById(1).Returns(checklist);
            _repo.AddItem(Arg.Any<ChecklistItem>()).Returns(ci =>
            {
                var i = ci.Arg<ChecklistItem>();
                i.ItemId = 1;
                return i;
            });

            var result = _sut.AddItem(1, item);

            result.Text.Should().Be("Write unit tests");
            result.IsCompleted.Should().BeFalse();
            result.ChecklistId.Should().Be(1);
        }

        // ── ToggleItem ────────────────────────────────────────────────────────

        [Test]
        public void ToggleItem_WhenFalse_SetsToTrue()
        {
            var item = new ChecklistItem { ItemId = 1, IsCompleted = false };
            _repo.GetItemById(1).Returns(item);
            _repo.UpdateItem(Arg.Any<ChecklistItem>());

            _sut.ToggleItem(1);

            _repo.Received(1).UpdateItem(Arg.Is<ChecklistItem>(i => i.IsCompleted == true));
        }

        [Test]
        public void ToggleItem_WhenTrue_SetsToFalse()
        {
            var item = new ChecklistItem { ItemId = 1, IsCompleted = true };
            _repo.GetItemById(1).Returns(item);
            _repo.UpdateItem(Arg.Any<ChecklistItem>());

            _sut.ToggleItem(1);

            _repo.Received(1).UpdateItem(Arg.Is<ChecklistItem>(i => i.IsCompleted == false));
        }

        // ── GetChecklistProgress ──────────────────────────────────────────────

        [Test]
        public void GetChecklistProgress_AllComplete_Returns100()
        {
            var items = new List<ChecklistItem>
            {
                new() { IsCompleted = true },
                new() { IsCompleted = true },
                new() { IsCompleted = true }
            };
            _repo.GetItemsByChecklist(1).Returns(items);

            var result = _sut.GetChecklistProgress(1);

            result.Should().Be(100.0);
        }

        [Test]
        public void GetChecklistProgress_HalfComplete_Returns50()
        {
            var items = new List<ChecklistItem>
            {
                new() { IsCompleted = true  },
                new() { IsCompleted = false }
            };
            _repo.GetItemsByChecklist(1).Returns(items);

            var result = _sut.GetChecklistProgress(1);

            result.Should().Be(50.0);
        }

        [Test]
        public void GetChecklistProgress_NoItems_ReturnsZero()
        {
            _repo.GetItemsByChecklist(1).Returns(new List<ChecklistItem>());

            var result = _sut.GetChecklistProgress(1);

            result.Should().Be(0.0);
        }
    }
}
