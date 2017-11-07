ProjectX Module for Fougerite!

it's module is full plugins for a great server rust.



Help extension:

- TimerEdit

//active reference
using TimerEdit;

// !! IMPORTANT !!!
// This is already activated in ProjectX

    //https://github.com/wilywork/ProjectX/blob/master/ProjectX/ProjectX.cs#L390
    //for start the timer
    TimerEditStart.Init();

    //https://github.com/wilywork/ProjectX/blob/master/ProjectX/ProjectX.cs#L483
    //for stop/clear the timer
    TimerEditStart.DeInitialize();

// !! AND IMPORTANT !!!

//Create timer  delay=seconds  callback=action/function
int delay = 10;
TimerEvento.Once(delay, () => {
    //your code for executed!
    //the code here will be executed in 10 seconds
});

//Create timer with repetitions  dalay=seconds  reps=0(infinit)  callback=action/function

    TimerEvento.Repeat(delay, 0, () => {
        ProjectX.BroadCast(ProjectX.configServer.NameServer,"timer repeat infinite.");
    });

    TimerEvento.Repeat(delay, 5, () => {
        ProjectX.BroadCast(ProjectX.configServer.NameServer,"timer 5 repeat.");
    });
