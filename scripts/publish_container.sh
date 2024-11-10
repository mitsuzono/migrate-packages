#!/bin/bash

# ./publish_container.sh CSV_PATH NEW_ORG_NAME NEW_PACKAGE_NAME NEW_REPO_NAME USER_NAME PAT
csv_path=$1
new_org=$2
new_package_name=$3
new_repo_name=$4
user=$5
pat=$6

# docker login
export CR_PAT=$pat
echo $CR_PAT | docker login ghcr.io -u $user --password-stdin

while read row; do
  echo $row

  id=`echo ${row} | cut -d , -f 1`
  tag=`echo ${row} | cut -d , -f 2`
  # version=`echo ${row} | cut -d , -f 3`
  # created_at=`echo ${row} | cut -d , -f 4`
  commit_id=`echo ${row} | cut -d , -f 5`
  dockerfile_path=`echo ${row} | cut -d , -f 6`

  image_name=ghcr.io/$new_org/$new_package_name:$tag
  echo "${image_name}"

  # git checkout
  git checkout $commit_id

  # docker build
  docker build -t $image_name $dockerfile_path --label "org.opencontainers.image.source=https://github.com/${new_org}/${new_repo_name}"

  # docker push
  docker push $image_name
done < packages.csv
