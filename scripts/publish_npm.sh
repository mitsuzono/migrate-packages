#!/bin/bash

# ./publish_npm.sh CSV_PATH OLD_ORG_NAME OLD_REPO_NAME NEW_ORG_NAME NEW_REPO_NAME PAT
csv_path=$1
old_org=$2
old_repo_name=$3
new_org=$4
new_repo_name=$5
pat=$6

while read row; do
  echo $row

  # read columns
  id=`echo ${row} | cut -d , -f 1`
  tag=`echo ${row} | cut -d , -f 2`
  # version=`echo ${row} | cut -d , -f 3`
  # created_at=`echo ${row} | cut -d , -f 4`
  commit_id=`echo ${row} | cut -d , -f 5`
  packagejson_path=`echo ${row} | cut -d , -f 6`

  # git checkout
  git checkout $commit_id

  # edit package.json
  sed -i -e "s/${old_org}/${new_org}/" package.json
  sed -i -e "s/${old_repo_name}/${new_repo_name}/" package.json
  cat package.json

  # publish
  npm publish

  # reset package.json
  git restore package.json
  git restore package-lock.json
  rm -rf node_modules
done < $csv_path
