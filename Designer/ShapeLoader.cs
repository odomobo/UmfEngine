using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Designer
{
    internal static class ShapeLoader
    {
        internal static void LoadShape(ref List<Vector2> oldShape)
        {
            var input = @"    new Vector2(-1.1999998f, -1f),
    new Vector2(-0.99999976f, -1.2f),
    new Vector2(-0.09999993f, -1.2f),
    new Vector2(0.19999999f, -0.7f),
    new Vector2(1.3000002f, -0.7f),
    new Vector2(1.4000002f, -0.8f),
    new Vector2(1.8000002f, -0.8f),
    new Vector2(2.1f, -0.5f),
    new Vector2(2.1f, -0.3f),
    new Vector2(1.8000002f, 0f),
    new Vector2(-1.0999998f, 0f),
    new Vector2(-1.1999998f, -0.1f),";
            var result = ShowInputDialog(ref input);

            if (result == DialogResult.OK)
            {
                var newShape = new List<Vector2>();

                // so far so good; try to parse
                var regex = new Regex(@"new\s+Vector2\s*\(\s*(-?[\d\._]+)f\s*,\s*(-?[\d\._]+)f\s*\)");
                var matchCollection = regex.Matches(input);
                foreach (Match match in matchCollection)
                {
                    if (!match.Success)
                    {
                        MessageBox.Show("Didn't match format");
                        return;
                    }
                    if (match.Groups.Count % 2 != 1)
                        throw new Exception("Regex should always match odd number of groups (including whole match)");

                    // start at 1, to exclude entire match
                    for (int i = 1; i < match.Groups.Count; i += 2)
                    {
                        if (!float.TryParse(match.Groups[i].Value, out var valX))
                        {
                            MessageBox.Show("Couldn't parse float");
                            return;
                        }
                        if (!float.TryParse(match.Groups[i + 1].Value, out var valY))
                        {
                            MessageBox.Show("Couldn't parse float");
                            return;
                        }
                        newShape.Add(new Vector2(valX, valY));
                    }
                }

                // finally, we are good!
                Program.UndoHistory.Add(oldShape.ToList()); // add a copy to the history
                oldShape = newShape;
            }
        }

        private static DialogResult ShowInputDialog(ref string input)
        {
            System.Drawing.Size size = new System.Drawing.Size(800, 800);
            Form inputBox = new Form();

            inputBox.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            inputBox.ClientSize = size;
            inputBox.Text = "Name";

            System.Windows.Forms.TextBox textBox = new TextBox();
            textBox.Size = new System.Drawing.Size(size.Width - 10, 750);
            textBox.Location = new System.Drawing.Point(5, 5);
            textBox.Multiline = true;
            textBox.Text = input;
            inputBox.Controls.Add(textBox);

            Button okButton = new Button();
            okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            okButton.Name = "okButton";
            okButton.Size = new System.Drawing.Size(75, 23);
            okButton.Text = "&OK";
            okButton.Location = new System.Drawing.Point(size.Width - 80 - 80, 775);
            inputBox.Controls.Add(okButton);

            Button cancelButton = new Button();
            cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new System.Drawing.Size(75, 23);
            cancelButton.Text = "&Cancel";
            cancelButton.Location = new System.Drawing.Point(size.Width - 80, 775);
            inputBox.Controls.Add(cancelButton);

            inputBox.AcceptButton = okButton;
            inputBox.CancelButton = cancelButton;

            DialogResult result = inputBox.ShowDialog();
            input = textBox.Text;
            return result;
        }
    }
}
