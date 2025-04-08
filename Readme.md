# Backend Candidate Challenge

This is a simple challenge to be used as part of an interview process for .NET backend developers. The challenge is intended to be done offline at your own speed using the tools of your choice. There is no single correct solution, just a set of tasks to trigger some relevant discussions. Be prepared to present your solution when we meet.   

#### Make a copy of this repo. Yes, we know that "Fork" exists, but please don't use it.

## Rules
1. **Keep the initial commits** free from your own changes, so we can use git log and git show to see what you did.
2. You are free to use any tool you like when developing. Important note: as we are targeting .NET 8 it's not possible to build the solution in VS 2019. Potential alternatives include **VS 2022** and **VS Code**.
3. You can use as much time as you like, but the intention is not to have you spend more than a couple of hours.
4. You are free to pull in any framework or libraries in order to solve the challenge, but be prepared to reason about your choices.
5. Use the patterns and practices you think are best on any part of the challenge, again be prepared to reason about your choices.
6. Commit your changes as you solve the various challenges and whenever you feel is a good time to commit. Let the commit message describe what part you solved.

## Background information
The code is a quiz solution based on .NET Core. You have a quiz client project and a quiz service project, both have corresponding test projects.

The solution uses a SQLite in-memory database with the following schema:
```
Quiz                           Question                        Answer
+-----------------+            +-----------------+            +-----------------+
| Id              |            | Id              |            | Id              |
| Title           |            | Text            |            | Text            |
|                 +------------+ QuizId          +------------+ QuestionId      |
|                 |            | CorrectAnswerId |            |                 |
|                 |            |                 |            |                 |
+-----------------+            +-----------------+            +-----------------+
```

## Challenge tasks
1. Clone a Git repository
2. Build the solution and make sure all tests pass before you make any changes.
3. Refactor the Get method in the QuizController to an architecture of your choice making the controller easier to test and maintain.
4. Create a new test where you create a quiz with minimum two questions as testdata for the test, take the quiz and assert that you have the correct score based on number of correct answers. 1 point per correct answer.
5. Do a code review and add **TODO** comments in the code where you see things that you would fix or do differently e.g.  //TODO I prefer x over z
6. Once you are done, push your changes and send URL to your repository back to the person that sent you the challenge. If you need a hint on git - see [here](https://gist.github.com/SergeyAPetrov/ac34e742f7d00010ef3126635a066fb9).

That's it - best of luck!
