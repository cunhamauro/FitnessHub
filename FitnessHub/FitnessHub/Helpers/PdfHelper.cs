using FitnessHub.Data.Entities.GymMachines;
using FitnessHub.Data.Entities.Users;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace FitnessHub.Helpers
{
    public class PdfHelper : IPdfHelper
    {
        private readonly IWebHostEnvironment _hostEnvironment;

        public PdfHelper(IWebHostEnvironment hostEnvironment)
        {
            _hostEnvironment = hostEnvironment;
        }

        public MemoryStream GenerateWorkoutPdf(Client client, Instructor instructor, Workout model)
        {
            // PDF document
            var document = new PdfDocument();

            // Title
            document.Info.Title = $"{client.FirstName}'s Workout";

            // Empty page
            var page = document.AddPage();
            var gfx = XGraphics.FromPdfPage(page);

            // Load banner
            string imagePath = Path.Combine(_hostEnvironment.WebRootPath, "img", "fitnessHubBanner.png");
            XImage logo = XImage.FromFile(imagePath);

            // Banner dimensions
            double pageWidth = page.Width;
            double aspectRatio = logo.PixelHeight / (double)logo.PixelWidth;
            double bannerHeight = pageWidth * aspectRatio;

            // Draw the image on document
            gfx.DrawImage(logo, 0, 0, pageWidth, bannerHeight);

            // Set font
            var fontHeader = new XFont("Arial", 16, XFontStyleEx.Bold);
            var fontSubT = new XFont("Arial", 14, XFontStyleEx.Bold);
            var fontText = new XFont("Arial", 12, XFontStyleEx.Regular);
            var fontTextB = new XFont("Arial", 12, XFontStyleEx.Bold);
            var fontTextS = new XFont("Arial", 10, XFontStyleEx.Regular);



            // Track vertical position on the page
            double margin = 40;
            double yPosition = bannerHeight + margin;
            double lineHeight = 20; // Space between lines
            double pageHeight = page.Height - margin;

            // Client and instructor info
            gfx.DrawString($"{client.FirstName}'s Workout", fontHeader, XBrushes.Black, new XRect(margin, yPosition, pageWidth - margin * 2, lineHeight), XStringFormats.TopLeft);
            yPosition += lineHeight;

            gfx.DrawString($"{instructor.FullName}  [{instructor.Email}]", fontSubT, XBrushes.Gray, new XRect(margin, yPosition, pageWidth - margin * 2, lineHeight), XStringFormats.TopLeft);
            yPosition += lineHeight;

            // Client and instructor info
            gfx.DrawString($"@ {DateTime.UtcNow.ToLongDateString()}", fontTextS, XBrushes.Black, new XRect(margin, yPosition, pageWidth - margin * 2, lineHeight), XStringFormats.TopLeft);
            yPosition += lineHeight * 2;

            // Exercises
            gfx.DrawString($"Exercises:", fontHeader, XBrushes.OrangeRed, new XRect(margin, yPosition, pageWidth - margin * 2, lineHeight), XStringFormats.TopLeft);
            //yPosition += lineHeight;
            yPosition += lineHeight * 1.5;


            foreach (var exercise in model.Exercises)
            {
                // Check if there is enough space for the next block of exercise info
                if (yPosition + lineHeight * 6 > pageHeight)
                {
                    // Add a new page and reset graphics and yPosition
                    page = document.AddPage();
                    gfx = XGraphics.FromPdfPage(page);
                    yPosition = margin;
                }

                gfx.DrawString("Day of week: ", fontTextB, XBrushes.Gray, new XRect(margin, yPosition, pageWidth - margin * 2, lineHeight), XStringFormats.TopLeft);
                gfx.DrawString(exercise.DayOfWeek.ToString(), fontText, XBrushes.Black, new XRect(margin + 100, yPosition, pageWidth - margin * 2 - 100, lineHeight), XStringFormats.TopLeft);
                yPosition += lineHeight;

                gfx.DrawString("Exercise: ", fontTextB, XBrushes.Gray, new XRect(margin, yPosition, pageWidth - margin * 2, lineHeight), XStringFormats.TopLeft);
                gfx.DrawString(exercise.Name, fontText, XBrushes.Black, new XRect(margin + 100, yPosition, pageWidth - margin * 2 - 100, lineHeight), XStringFormats.TopLeft);
                yPosition += lineHeight;

                gfx.DrawString("Machine: ", fontTextB, XBrushes.Gray, new XRect(margin, yPosition, pageWidth - margin * 2, lineHeight), XStringFormats.TopLeft);
                gfx.DrawString(exercise.Machine.Name, fontText, XBrushes.Black, new XRect(margin + 100, yPosition, pageWidth - margin * 2 - 100, lineHeight), XStringFormats.TopLeft);
                yPosition += lineHeight;

                gfx.DrawString("Time: ", fontTextB, XBrushes.Gray, new XRect(margin, yPosition, pageWidth - margin * 2, lineHeight), XStringFormats.TopLeft);
                gfx.DrawString(exercise.Time.ToString(), fontText, XBrushes.Black, new XRect(margin + 100, yPosition, pageWidth - margin * 2 - 100, lineHeight), XStringFormats.TopLeft);
                yPosition += lineHeight;

                gfx.DrawString("Repetitions: ", fontTextB, XBrushes.Gray, new XRect(margin, yPosition, pageWidth - margin * 2, lineHeight), XStringFormats.TopLeft);
                gfx.DrawString(exercise.Repetitions.ToString(), fontText, XBrushes.Black, new XRect(margin + 100, yPosition, pageWidth - margin * 2 - 100, lineHeight), XStringFormats.TopLeft);
                yPosition += lineHeight;

                gfx.DrawString("Sets: ", fontTextB, XBrushes.Gray, new XRect(margin, yPosition, pageWidth - margin * 2, lineHeight), XStringFormats.TopLeft);
                gfx.DrawString(exercise.Sets.ToString(), fontText, XBrushes.Black, new XRect(margin + 100, yPosition, pageWidth - margin * 2 - 100, lineHeight), XStringFormats.TopLeft);
                yPosition += lineHeight * 1.4;

                exercise.Notes = $"* {exercise.Notes}";

                // Wrap and draw the notes content
                string notesContent = exercise.Notes;
                double notesMaxWidth = pageWidth - margin * 2; // Notes start at the left margin
                var notesLines = SplitTextIntoLines(gfx, notesContent, fontText, notesMaxWidth);

                foreach (string line in notesLines)
                {
                    // Check if adding this line will overflow the page
                    if (yPosition + lineHeight > pageHeight)
                    {
                        // Add a new page and reset graphics and yPosition
                        page = document.AddPage();
                        gfx = XGraphics.FromPdfPage(page);
                        yPosition = margin;
                    }

                    // Draw the line starting at the margin
                    gfx.DrawString(line, fontText, XBrushes.Black, new XRect(margin, yPosition, notesMaxWidth, lineHeight), XStringFormats.TopLeft);
                    yPosition += lineHeight;
                }

                // Add extra space after the notes
                yPosition += lineHeight * 0.1;

                double lineHeightThickness = 3; // Thickness of the orange line
                double lineYPosition = yPosition + lineHeight / 2 - lineHeightThickness / 2; // Center the line in the middle of the space
                gfx.DrawLine(new XPen(XColor.FromArgb(255, 98, 1), lineHeightThickness), margin, lineYPosition, pageWidth - margin, lineYPosition);

                yPosition += lineHeight * 1.6;
            }

            // Save the document to a MemoryStream
            var memoryStream = new MemoryStream();
            document.Save(memoryStream, false);
            memoryStream.Position = 0;

            return memoryStream; // Return the PDF memory stream
        }

        private List<string> SplitTextIntoLines(XGraphics gfx, string text, XFont font, double maxWidth)
        {
            var lines = new List<string>();
            var words = text.Split(' ');
            var currentLine = "";

            foreach (var word in words)
            {
                string testLine = string.IsNullOrEmpty(currentLine) ? word : currentLine + " " + word;
                double width = gfx.MeasureString(testLine, font).Width;

                if (width <= maxWidth)
                {
                    currentLine = testLine;
                }
                else
                {
                    lines.Add(currentLine);
                    currentLine = word;
                }
            }

            if (!string.IsNullOrEmpty(currentLine))
            {
                lines.Add(currentLine);
            }

            return lines;
        }
    }
}
