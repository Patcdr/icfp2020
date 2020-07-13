const { JSDOM } = require("jsdom");
const fs = require("fs");
const fetch = require('node-fetch');

const URL_AUTH = process.env.URL_AUTH;

function Blob(data) {
	this.data = data;
}

function FileReader() {}
FileReader.prototype.readAsText = function(blob) {
	this.result = blob.data+"";
	this.onloadend();
}

module.exports = {
	lambda: async function (event) {
		let problem = event.problem;
		let solution = event.solution || '';
		let boosters = event.boosters || '';

		let headers = {'Authorization': 'Basic ' + URL_AUTH};

		if (problem.startsWith('http')) {
			console.log('fetch');
			const response = await fetch(problem, {headers});
			problem = await response.text();
		}

		if (solution.startsWith('http')) {
			const response = await fetch(solution, {headers});
			solution = await response.text();
		}

		if (boosters.startsWith('http')) {
			const response = await fetch(boosters, {headers});
			boosters = await response.text();
		}

		try {
			if (problem[0] =='(') {
				console.log("Grading solution")
				return await module.exports.grade(problem, solution, boosters);
			} else {
				console.log("Validating puzzle")
				return await module.exports.validatePuzzle(problem, solution);
			}
		} catch (e) {
			console.error('Fail: ' + e);
			return 'Fail: ' + e;
		}
	},
	grade: function(task, solution, boosters) {
		return new Promise(function(resolve, reject){
			eval(fs.readFileSync("lambda.js")+"");

			let dom = new JSDOM('<div id="main_section"></div>');
			let window = dom.window;
			validate();

			var todo = 2;
			function done() {
				if (!--todo) {
					window.execute_solution.click();
				}
			}

			Object.defineProperty(window.output, 'textContent', {
				set: function(text) {
					console.log(text);
					if (text.startsWith("Done uploading ")) {
						done();
					}
					if (text.startsWith("Failed")) {
						reject(text);
					}
					var match;
					if (match = text.match(/Your solution took (\d+) time units/)) {
						resolve(match[1]);
					}
				},
			})

			Object.defineProperty(window.submit_task, 'files', {
				value: [new Blob(task)],
			});

			Object.defineProperty(window.submit_solution, 'files', {
				value: [new Blob(solution)],
			});

			Object.defineProperty(window.submit_boosters, 'files', {
				value: [new Blob(boosters)],
			});

			window.submit_boosters.onchange();
			window.submit_task.onchange();
			window.submit_solution.onchange();
		});
	},
	validatePuzzle: function(puzzleCond, task) {
		return new Promise(function(resolve, reject) {
			eval(fs.readFileSync("lambda.js")+"");

			let dom = new JSDOM('<div id="main_section"></div>');
			let window = dom.window;
			puzzle();

			Object.defineProperty(window.output, 'textContent', {
				set: function(text) {
					if (text.startsWith("Cannot check: ")) {
						// Loading is async and we don't get any feedback about when it's ready.
						// Just retry every ms until it loads.
						// Malformed files will still get caught by the reject below.
						setTimeout(() => window.execute_solution.click(), 1);
						console.log("Loading...");
						return;
					}
					console.log(text);
					if (text.startsWith("Success")) {
						resolve("100");
					}
					if (text.startsWith("Failed")) {
						reject(text);
					}
				},
			})

			Object.defineProperty(window.submit_task, 'files', {
				value: [new Blob(puzzleCond)],
			});

			Object.defineProperty(window.submit_solution, 'files', {
				value: [new Blob(task)],
			});

			window.submit_task.onchange();
			window.submit_solution.onchange();
			window.execute_solution.click();
		});
	},
};

if (require.main === module) {
	let args = process.argv.slice(2);
	if (args.length < 2 || args.length > 3) {
		console.log("usage: node grader.js prob-NNN.desc prob-NNN.sol [boosters.boost]")
		process.exit(1);
	}

	module.exports.lambda({
		problem: fs.readFileSync(args[0], {encoding: 'utf-8'}),
		solution: fs.readFileSync(args[1], {encoding: 'utf-8'}),
		boosters: args.length > 2 ? fs.readFileSync(args[2], {encoding: 'utf-8'}) : '',
	}).then(function(score) {
		console.log("Score:", score);
	}, function(err) {
		process.exit(1);
	})
}
