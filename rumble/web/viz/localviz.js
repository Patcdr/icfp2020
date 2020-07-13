function Blob(data) {
	this.data = data;
}
window.Blob = Blob;

function FileReader() {}
FileReader.prototype.readAsText = function(blob) {
	this.result = blob.data+"";
	this.onloadend();
}
window.FileReader = FileReader;

function fromHash() {
	var hash = decodeURIComponent(location.hash);
	var idx = 0;
	for (var i = 0; i < 4; i++) {
		idx = hash.indexOf('#', idx+1);
		if (idx == -1) {
			return;
		}
	}

	var task = hash.substr(1, idx-1);
	var solution = hash.substr(idx+1);
	return {
		task: task,
		solution: solution,
		boosters: '',
	};
}

async function maybeFetch(url) {
	if (!url) {
		return '';
	}
	return await fetch(url).then((resp) => resp.text());
}

async function fromSearch() {
	let params = new URLSearchParams(location.search);
	let task = maybeFetch(params.get("task"));
	let solution = maybeFetch(params.get("solution"));
	let boosters = maybeFetch(params.get("boosters"));
	return {
		task: await task,
		solution: await solution,
		boosters: await boosters,
	};
}

var logCallback = () => {};
function hookCanvas(cb) {
	logCallback = cb;
}

window.beforeRender = function() {
	var canvas = document.getElementById("canvas");
	var ctx = canvas.getContext("2d");
	var origFillText = ctx.fillText;
	Object.defineProperty(ctx, 'fillText', {
		value: function() {
			origFillText.apply(ctx, arguments);
			logCallback(arguments[0]);
		},
		configurable: true,
	});
	Object.defineProperty(canvas, 'getContext', {
		value: function(type) {
			console.log("getContext");
			return ctx;
		}
	});
};

function loadAndRun(task, solution, boosters) {
	console.log("task", task);
	console.log("solution", solution);
	console.log("boosters", boosters);

	Object.defineProperty(window.submit_task, 'files', {
		value: [new Blob(task)],
		configurable: true,
	});

	Object.defineProperty(window.submit_solution, 'files', {
		value: [new Blob(solution)],
		configurable: true,
	});

	Object.defineProperty(window.submit_boosters, 'files', {
		value: [new Blob(boosters)],
		configurable: true,
	});

	window.submit_task.onchange();
	window.submit_solution.onchange();
	window.submit_boosters.onchange();

	window.submit_task.parentNode.style.display = 'none';
	window.submit_solution.parentNode.style.display = 'none';
	window.submit_boosters.parentNode.style.display = 'none';
}

function shuffleArray(array) {
    for (var i = array.length - 1; i > 0; i--) {
        var j = Math.floor(Math.random() * (i + 1));
        var temp = array[i];
        array[i] = array[j];
        array[j] = temp;
    }
}

async function attractMode() {
	AWS.config.region = 'us-west-2';
	AWS.config.accessKeyId = 'AKIAV3HLA4UABUU5HH6D';
	AWS.config.secretAccessKey = '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';
	const dynamo = new AWS.DynamoDB();
	let query = {
		ScanIndexForward: false,
		ExpressionAttributeValues: {':v' : {S: 'thecat'}},
		ExpressionAttributeNames: {'#pk': 'hash'},
		KeyConditionExpression: '#pk = :v',
		TableName: 'RumbleJob',
		Limit: 500,
	};
	let result = await dynamo.query(query).promise();
	shuffleArray(result.Items);
	let job = result.Items.pop();
	while (job.score.N == 0 || job.entrant.S == 'MapMaker') {
		job = result.Items.pop();
	}
	let entrant = job.entrant.S;
	let tag = job.tag.S;
	let problem = job.problem.S;
	let runtime = parseInt(job.runtime.N, 10);
	let score = job.score.N;
	let id = job.id.S;
	window.entrant_heading.textContent = entrant;
	window.tag_heading.textContent = tag;
	window.problem_heading.textContent = problem;
	window.runtime_heading.textContent = new Date(runtime).toLocaleString();
	window.score_heading.textContent = score + " ticks";
	console.log(entrant, tag, problem, runtime, score, id);

	let done = 0;
	let sleeping = false;
	let started = false;
	hookCanvas((text) => {
		if (text.startsWith("Done uploading ")) {
			done++;
			console.log(text);
			if (done == 2) {
				setTimeout(() => window.execute_solution.click(), 10);
			}
		}
		if (text.startsWith("Press SPACE ") && !started) {
			started = true;
			setTimeout(() => window.onkeypress({
				preventDefault: function() {},
				keyCode: 32,
			}), 10);
		}
		if (text.startsWith("Success! ") || text.startsWith("Not all parts ") || text.startsWith("Failed: ")) {
			if (!sleeping) {
				sleeping = true;
				setTimeout(() => { location = location; }, 5000);
			}
		}
	});

	let task = await (await fetch("https://logs.rumbletoon.com/" + id + "/stdin")).text();
	let solution = await (await fetch("https://logs.rumbletoon.com/" + id + "/stdout")).text();
	loadAndRun(task, solution, '');
}

if (location.hash.length > 1) {
	window.afterRender = async function() {
		const {task, solution, boosters} = fromHash();
		loadAndRun(task, solution, boosters);
	}
} else if (location.search.length > 0) {
	window.afterRender = async function() {
		const {task, solution, boosters} = await fromSearch();
		loadAndRun(task, solution, boosters);
	}
} else {
	window.afterRender = function() {
		window.execute_solution.style.display = 'none';
		attractMode();
	}
}
