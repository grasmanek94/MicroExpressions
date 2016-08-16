using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MicroExpressions
{
    public partial class Form1 : Form
    {
        int correct_answers;
        int wrong_answers;
        int chosen_language;

        List<MicroExpression> micro_expressions;
        List<MicroExpression> todo_micro_expressions;

        Dictionary<Button, ExpressionAnswer> buttons;
        List<Button> button_list;

        MicroExpression current_expression;

        Random rng;

        public Form1()
        {
            InitializeComponent();
            rng = new Random();
            micro_expressions = new List<MicroExpression>();
            buttons = new Dictionary<Button, ExpressionAnswer>();
            todo_micro_expressions = new List<MicroExpression>();
            button_list = new List<Button>();

            button_list.Add(button1);
            button_list.Add(button2);
            button_list.Add(button3);
            button_list.Add(button4);
            button_list.Add(button5);
            button_list.Add(button6);
            button_list.Add(button7);

            foreach(Button button in button_list)
            {
                button.Click += answer_button_Click;
            }

            int i = 0;
            int errors = 0;
            while (true)
            {
                ++i;
                try
                {
                    MicroExpression expression = new MicroExpression(i, facebox);
                    micro_expressions.Add(expression);
                    expression.ImageShowStarted += Expression_ImageShowStarted;
                    expression.ImageShowCompleted += Expression_ImageShowCompleted;
                }
                catch(Exception)
                {
                    if (++errors == 16)
                    {
                        break;
                    }
                }
            }

            correct_answers = 0;
            wrong_answers = 0;
            chosen_language = 0;
            current_expression = null;

            ChaneButtonsStatus(false);
            RandomizeButtons();
        }

        private void RandomizeButtons()
        {
            buttons.Clear();

            List<ExpressionAnswer> expression_answers = Enum.GetValues(typeof(ExpressionAnswer)).Cast<ExpressionAnswer>().ToList();
            while(expression_answers.Count > 0)
            {
                int num = rng.Next(expression_answers.Count);
                Button button = button_list[buttons.Count];
                ExpressionAnswer answer = expression_answers[num];
                expression_answers.RemoveAt(num);
                buttons.Add(button, answer);
            }
            ChangeButtonsLanguage(chosen_language);
        }

        private void ChaneButtonsStatus(bool enabled)
        {
            foreach (Button button in button_list)
            {
                button.Enabled = enabled;
            }
        }

        private void ChangeButtonsLanguage(int language)
        {
            if (language < 2 && language >= 0)
            {
                chosen_language = language;
                foreach (var kp_button_answer in buttons)
                {
                    kp_button_answer.Key.Text = Translation.ExpressionAnswerTranslations[(int)kp_button_answer.Value, language];
                }

                change_language_button.Text = Translation.ExpressionAnswerTranslations[7, language];
            }
        }

        private void time_counter_Scroll(object sender, EventArgs e)
        {
            selected_time_label.Text = time_counter.Value.ToString() + " ms";
        }

        private void change_language_button_Click(object sender, EventArgs e)
        {
            ChangeButtonsLanguage(chosen_language ^ 1);
        }

        private void UpdateLabels()
        {
            correct_counter.Text = correct_answers.ToString();
            wrong_counter.Text = wrong_answers.ToString();
            total_counter.Text = (correct_answers + wrong_answers).ToString();
        }

        private void start_button_Click(object sender, EventArgs e)
        {
            correct_answers = 0;
            wrong_answers = 0;
            UpdateLabels();

            todo_micro_expressions.Clear();

            foreach (MicroExpression expression in micro_expressions)
            {
                todo_micro_expressions.Add(expression);
            }

            RandomizeButtons();
            StartShowingImage();
        }

        private void StartShowingImage()
        {
            if (todo_micro_expressions.Count > 0)
            {
                int num = rng.Next(todo_micro_expressions.Count);
                current_expression = todo_micro_expressions[num];
                todo_micro_expressions.RemoveAt(num);
                current_expression.Show(1000, 3000, time_counter.Value);
            }
            else
            {
                facebox.Image = null;
                facebox.Refresh();
                ChaneButtonsStatus(false);
            }
        }

        private void Expression_ImageShowStarted(MicroExpression sender, EventArgs e)
        {
            ChaneButtonsStatus(false);
        }

        private void Expression_ImageShowCompleted(MicroExpression sender, EventArgs e)
        {
            ChaneButtonsStatus(true);
        }

        private void answer_button_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            ExpressionAnswer answer = buttons[button];

            if (current_expression.Answer(answer))
            {
                ++correct_answers;
            }
            else
            {
                ++wrong_answers;
            }

            UpdateLabels();
            StartShowingImage();
        }

    }
}
