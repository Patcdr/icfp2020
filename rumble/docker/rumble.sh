#!/bin/bash
# set -o xtrace
# set -e

# Pull down the input content from the given URL
ENTRANT=${1:-Submission}
INPUT_URL=${2:-https://www.rumbletoon.com/problems/smoke/prob-001.desc}
GROUP=$(echo $INPUT_URL | sed 's#^.*/\(.*\)/.*$#\1#')
PROBLEM=$(echo $INPUT_URL | sed 's#.*/##')
GIT_TAG_SHORT=$(echo $GIT_TAG | head -c 7)

export ENTRANT
export INPUT_URL
export GROUP
export PROBLEM

mkdir -p out_files

echo "Rumbling $ENTRANT against $INPUT_URL"

curl -H "authorization: Basic dGhlY2F0OmxvbHpsb2x6" -s $INPUT_URL | tee out_files/stdin
echo

cat entrants/$ENTRANT | tee out_files/cmd
echo

START_TIME="$(date +%s%N | cut -b1-13)"
cat out_files/stdin | `envsubst < out_files/cmd` > >(tee out_files/stdout) 2> >(tee out_files/stderr | cat 1>&2)
END_TIME="$(date +%s%N | cut -b1-13)"
ELAPSED="$(($END_TIME-$START_TIME))"

cat out_files/stdout

SCORE="$(cat out_files/stderr | grep SCORE: |grep -o [0-9]*)"
SCORE=${SCORE:-1}

aws configure set metadata_service_num_attempts 50

aws --region us-west-2 dynamodb put-item --table-name RumbleJob --item \
'{ "id": {"S": "'$AWS_BATCH_JOB_ID'"},'\
'"hash": {"S": "thecat"},'\
'"runtime": {"N": "'`date +%s%N | cut -b1-13`'"},'\
'"entrant": {"S": "'$ENTRANT'"},'\
'"score": {"N": "'$SCORE'"},'\
'"tag": {"S": "'$GIT_TAG_SHORT'"},'\
'"group": {"S": "'$GROUP'"},'\
'"problem": {"S": "'$PROBLEM'"},'\
'"elapsed": {"N": "'$ELAPSED'"} }'

aws s3 sync out_files/ s3://$S3_LOGS_BUCKET/$AWS_BATCH_JOB_ID/ --content-type=text/plain

aws lambda invoke --region us-west-2 \
    --invocation-type Event \
    --function-name validate \
    --payload '{"id": "'$AWS_BATCH_JOB_ID'"}' out
