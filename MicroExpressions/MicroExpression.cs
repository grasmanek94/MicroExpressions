using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace MicroExpressions
{
    public class MicroExpression
    {
        public delegate void MicroExpressionShownHandler(MicroExpression sender, EventArgs e);
        public event MicroExpressionShownHandler ImageShowCompleted;
        public event MicroExpressionShownHandler ImageShowStarted;

        private static readonly string resources_folder = "expressions";

        public int Id { get; private set; }
        private Bitmap Expression { get; set; }
        private Bitmap Face { get; set; }
        private ExpressionAnswer CorrectAnswer { get; set; }
        private Timer timer;
        private int current_show_step;
        private PictureBox picture_box;
        private Random rng;
        public int MinimumTime { get; private set; }
        public int MaximumTime { get; private set; }
        public int ExpressionTime { get; private set; }

        public MicroExpression(int id, PictureBox picture_box)
        {
            Id = id;

            timer = new Timer();
            timer.Tick += Timer_Tick;

            rng = new Random();

            this.picture_box = picture_box;

            Regex reg = new Regex(@"/" + id.ToString() + @"[ab](SUR|SAD|FEA|CON|ANG|DIS|HAP).png");

            List<string> files = Directory.GetFiles(resources_folder + "/", "*.png")
                .Where(path => reg.IsMatch(path))
                .ToList();

            if (files.Count != 2)
            {
                throw new Exception("Expression " + id.ToString() + " does not have exactly 2 images (a and b), actual amount of images: " + files.Count.ToString());
            }

            string first = files[0];
            string second = files[1];

            switch (first[first.Length - 8])
            {
                case 'a':
                    Face = (Bitmap)Image.FromFile(first);
                    Expression = (Bitmap)Image.FromFile(second);
                    break;

                case 'b':
                    Face = (Bitmap)Image.FromFile(second);
                    Expression = (Bitmap)Image.FromFile(first);
                    break;
            }

            CorrectAnswer = (ExpressionAnswer)Enum.Parse(typeof(ExpressionAnswer), first.Substring(first.Length - 7, 3));
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            switch (current_show_step)
            {
                case 0:
                    timer.Interval = ExpressionTime;
                    picture_box.Image = Expression;
                    picture_box.Refresh();
                    current_show_step = 1;
                    break;
                case 1:
                    timer.Interval = 500;
                    picture_box.Image = Face;
                    picture_box.Refresh();
                    current_show_step = 2;
                    break;
                case 2:
                    timer.Stop();
                    ImageShowCompleted?.Invoke(this, e);
                    break;
            }
        }

        ~MicroExpression()
        {
            Expression.Dispose();
            Face.Dispose();
        }

        public void Show(int min_time, int max_time, int expression_time)
        {
            MinimumTime = min_time;
            MaximumTime = max_time;
            ExpressionTime = expression_time;

            if (MaximumTime < MinimumTime)
            {
                throw new Exception("MaximumTime cannot be less than MinimumTime");
            }

            current_show_step = 0;
            timer.Interval = MinimumTime + rng.Next(MaximumTime - MinimumTime);
            timer.Start();
            picture_box.Image = Face;
            picture_box.Refresh();
            ImageShowStarted?.Invoke(this, new EventArgs());
        }

        public bool Answer(ExpressionAnswer answer)
        {
            return answer == CorrectAnswer;
        }
    }

    public static class Translation
    {
        public static readonly string[,] ExpressionAnswerTranslations = new string[8, 2]
        {
            {"Surprised", "Verrast" },
            {"Sadness", "Verdriet" },
            {"Fear", "Angst" },
            {"Contempt", "Minachting" },
            {"Anger", "Woede" },
            {"Disgust", "Walging" },
            {"Happiness", "Vrolijkheid" },
            {"Verander naar NEDERLANDS", "Change to ENGLISH" },
            //en waar is TELEURGESTELD?!
        };
    }

    public enum ExpressionAnswer
    {
        SUR,
        SAD,
        FEA,
        CON,
        ANG,
        DIS,
        HAP
    }
}
