Text-Captcha-Solver
===================

C# Library to solve simple logic questions, I have Created For Learning Purpose.

the Library solve varaious types of questions...




Usage:

         TextCaptchaSolver.CaptchaSolver solver = new TextCaptchaSolver.CaptchaSolver();
            try
            {
                return solver.Solve(input);
            }
            catch(Exception ex)
            {
                return null;
            }
                
