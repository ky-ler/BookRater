using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BookRater.Models;

public class BookReview : IValidatableObject
{
    [Key]
    public int Id { get; set; }

    [Required]
    [DisplayName("Book Title")]
    public string Title { get; set; } = null!;

    // user ID from AspNetUser table.
    [DisplayName("Reviewer")]
    public string? OwnerID { get; set; }


    [Range(0, 5, ErrorMessage = "Rating must be between 0 and 5.")]
    public int Rating { get; set; }

    [DisplayName("Date Rated")]
    [DataType(DataType.Date)]
    public DateTime DateRated { get; set; } = DateTime.Now;

    [DisplayName("Started")]
    [DataType(DataType.Date)]
    public DateTime DateStartedRead { get; set; }

    [DisplayName("Finished")]
    [DataType(DataType.Date)]
    public DateTime DateCompletedRead { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DateStartedRead > DateCompletedRead)
        {
            yield return new ValidationResult(
                "Started Reading date cannot be after Finished Reading date",
                new[] { nameof(DateStartedRead), nameof(DateCompletedRead) });
        }

        if (DateStartedRead > DateTime.Now.Date)
        {
            yield return new ValidationResult(
                "Started Reading date cannot be in the future",
                new[] { nameof(DateStartedRead) });
        }
        if (DateCompletedRead > DateTime.Now.Date)
        {
            yield return new ValidationResult(
                "Finished Reading date cannot be in the future",
                new[] { nameof(DateCompletedRead) });
        }
    }
}