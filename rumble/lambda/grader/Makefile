all: node_modules lambda.js

node_modules:
	npm install js-beautify jsdom

lambda.js: node_modules
	curl https://icfpcontest2019.github.io/static/lambda.js | ./node_modules/.bin/js-beautify > lambda.js
