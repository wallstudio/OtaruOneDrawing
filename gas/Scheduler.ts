
function onTrigger() { main(new Date()); }

function main(date : Date = new Date())
{
	const span = 5;
	const calender = CalendarApp.getCalendarById("22326mbg1cbd88bi5s7e9cklbg@group.calendar.google.com");
	const searchStart = new Date(date.getFullYear(), date.getMonth() - 1, date.getDate());
	const searchEnd = new Date(date.getFullYear(), date.getMonth() + 1, date.getDate());
	const events = calender.getEvents(searchStart, searchEnd);

	const previousEvent = events.reverse().find(i => i.getStartTime().getDate() < date.getDate());
	const nextEvent = events.find(i => i.getEndTime().getDate() > date.getDate());
	const toDayEvent = events.find(i => i.getStartTime().getDate() == date.getDate() || i.getEndTime().getDate() == date.getDate());
	const runningEvent = events.find(i => approximately(i.getStartTime(), span, date) >= 0  && approximately(i.getEndTime(), span, date) <= 0);
	console.log(`previousEvent: ${previousEvent?.getTitle()} ${previousEvent?.getStartTime()}-${previousEvent?.getEndTime()}`);
	console.log(`nextEvent: ${nextEvent?.getTitle()} ${nextEvent?.getStartTime()}-${nextEvent?.getEndTime()}`);
	console.log(`toDayEvent: ${toDayEvent?.getTitle()} ${toDayEvent?.getStartTime()}-${toDayEvent?.getEndTime()}`);
	console.log(`runningEvent: ${runningEvent?.getTitle()} ${runningEvent?.getStartTime()}-${runningEvent?.getEndTime()}`);

	const morningTime = new Date(toDayEvent?.getStartTime()?.getFullYear(), toDayEvent?.getStartTime()?.getMonth(), toDayEvent?.getStartTime()?.getDate(), 9, 30);
	const accumulateTime = new Date(previousEvent?.getStartTime()?.getFullYear(), previousEvent?.getStartTime()?.getMonth(), previousEvent?.getStartTime()?.getDate(), 36, 45);
	if(toDayEvent != null
		&& 0 == approximately(morningTime, span, date))
	{
		run("NotificationMorning", morningTime, toDayEvent.getStartTime(), nextEvent?.getStartTime());
	}
	else if(runningEvent != null
		&& 0 == approximately(runningEvent.getStartTime(), span, date))
	{
		run("NotificationStart", runningEvent.getStartTime(), runningEvent.getStartTime(), nextEvent?.getStartTime());
	}
	else if(runningEvent != null
		&& 0 == approximately(runningEvent.getEndTime(), span, date))
	{
		run("NotificationFinish", runningEvent.getEndTime(), runningEvent.getStartTime(), nextEvent?.getStartTime());
	}
	else if(previousEvent?.getStartTime()?.getDate() == date.getDate() - 1
		&& 0 == approximately(accumulateTime, span, date))
	{
		run("AccumulationPosts", accumulateTime, previousEvent.getStartTime(), nextEvent?.getStartTime());
	}
	else
	{
		console.log("no task");
	}
}

function run(workflowName : string, actionDate : Date | GoogleAppsScript.Base.Date, eventDate : Date | GoogleAppsScript.Base.Date, nextDate : Date | GoogleAppsScript.Base.Date) : void
{
	console.log(`workflow:${workflowName}, actionDate: ${actionDate}, eventDate:${eventDate}, nextDate:${nextDate}`);
	const headers = {
			Authorization : `token ${getGithubToken()}`,
			Accept : "application/vnd.github.v3+json"
	};
	const workflowsResponse = UrlFetchApp.fetch(`https://api.github.com/repos/wallstudio/MakiOneDrawingBot/actions/workflows`, { method: "get", headers: headers });
	const workflows : any[] = JSON.parse(workflowsResponse.getContentText()).workflows;
	const workflow = workflows.find(w => w.name == "Main");

	let res = UrlFetchApp.fetch(`https://api.github.com/repos/wallstudio/MakiOneDrawingBot/actions/workflows/${workflow.id}/dispatches`,
	{
		method: "post",
		headers: headers,
		payload: JSON.stringify({ ref: "master", inputs:
		{
			command: workflowName,
			actionDate : `${actionDate.getFullYear()}/${actionDate.getMonth() + 1}/${actionDate.getDate()} ${actionDate.getHours()}:${actionDate.getMinutes()} +09:00`,
			eventDate: `${eventDate.getFullYear()}/${eventDate.getMonth() + 1}/${eventDate.getDate()} ${eventDate.getHours()}:${eventDate.getMinutes()} +09:00`,
			nextDate: `${nextDate?.getFullYear()}/${nextDate?.getMonth() + 1}/${nextDate?.getDate()} ${nextDate?.getHours()}:${nextDate?.getMinutes()} +09:00`,
			general: "From GAS trigger."
		}})
	});
	console.log(`${res.getResponseCode()}`);
}

function approximately(ref : Date | GoogleAppsScript.Base.Date, span : number, date : Date) : number
{
	const dateStart = new Date(date.getFullYear(), date.getMonth(), date.getDate(), date.getHours(), date.getMinutes());
	const dateEnd = new Date(date.getFullYear(), date.getMonth(), date.getDate(), date.getHours(), date.getMinutes() + span);
	if(dateStart <= ref && ref < dateEnd)
	{
		return 0;
	}
	else if (ref < dateStart)
	{
		return +1;
	}
	else if (dateEnd <= ref)
	{
		return -1;
	}
}
