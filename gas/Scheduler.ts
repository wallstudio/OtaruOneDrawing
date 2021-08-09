function init() : void
{
  schedule("notificationMorning", new Date(), 9, 30);
}

function reset() : void
{
  schedule("notificationMorning", getNext(new Date()), 9, 30);
}

function notificationMorning() : void
{
  // 09:30
  const date = new Date();
  run("NotificationMorning", date, getNext(date));
  schedule("notificationStart", date, 22, 0);
}

function notificationStart() : void
{
  // 22:00
  const date = new Date();
  run("NotificationStart", date, getNext(date));
  schedule("notificationFinish", date, 25, 0);
}

function notificationFinish() : void
{
  // 25:00 (01:00)
  const date = getYesterday();
  run("NotificationFinish", date, getNext(date));
  schedule("accumulationPosts", date, 36, 45);
}

function accumulationPosts() : void
{
  // 36:45 (12:45)
  const date = getYesterday();
  run("AccumulationPosts", date, getNext(date));
  reset();
}


function run(workflowName : string, date : Date, next : Date) : void
{
  const headers = {
      Authorization : `token ${getGithubToken()}`,
      Accept : "application/vnd.github.v3+json"
  };
  const workflowsResponse = UrlFetchApp.fetch(`https://api.github.com/repos/wallstudio/MakiOneDrawingBot/actions/workflows`, { method: "get", headers: headers });
  const workflows = JSON.parse(workflowsResponse.getContentText());
  const workflow = workflows.workflows.find(w => w.name == workflowName);

  let res = UrlFetchApp.fetch(`https://api.github.com/repos/wallstudio/MakiOneDrawingBot/actions/workflows/${workflow.id}/dispatches`,
  {
    method: "post",
    headers: headers,
    payload: JSON.stringify({ ref: "master", inputs: { date: fomatDate(date), next: fomatDate(next), general: "From GAS trigger." } })
  });
  console.log(res.getResponseCode());
}

function schedule(funcName : string, date : Date, h : number, m : number) : void
{
  const schedule = new Date(date.getFullYear(), date.getMonth(), date.getDate(), h, m);
  ScriptApp.newTrigger(funcName).timeBased().at(schedule).create();
  console.log(`Scheduled "${funcName}" at ${schedule.toLocaleString()}`);
}

function removeTrigger(funcName : string)
{
  const trigger = ScriptApp.getProjectTriggers().find(t => t.getHandlerFunction() == funcName);
  ScriptApp.deleteTrigger(trigger);
  console.log(`Removed "${funcName}"`);
}

function getYesterday() : Date
{
  let date = new Date();
  date.setDate(date.getDate() - 1);
  return date;
}

function getNext(date : Date) : Date
{
  let next = new Date(date.getFullYear(), date.getMonth(), date.getDate() + 1); // Tommorow
  while((next.getDate() % 10) % 3 != 0) next.setDate(next.getDate() + 1); // TODO:
  // while(next.getDate() % 10 != 3) next.setDate(next.getDate() + 1);
  return next;
}

function fomatDate(date : Date) : string
{
  return `${date.getFullYear()}/${date.getMonth()}/${date.getDate()}`;
}
