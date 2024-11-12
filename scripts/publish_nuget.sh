#!/bin/bash

# ./publish_nuget.sh CSV_PATH PACKAGE_NAME OLD_REPO_URL NEW_REPO_URL NEW_ORG_NAME USER PAT
csv_path=$1
package_name=$2
old_repo_url=$3
new_repo_url=$4
new_org=$5
user=$6
pat=$7

dotnet nuget add source --username $user --password $pat --store-password-in-clear-text --name github-migrate "https://nuget.pkg.github.com/${new_org}/index.json"

while read row; do
  echo $row

  # read columns
  # id=`echo ${row} | cut -d , -f 1`
  # tag=`echo ${row} | cut -d , -f 2` # only container
  version=`echo ${row} | cut -d , -f 3`
  # created_at=`echo ${row} | cut -d , -f 4`
  commit_id=`echo ${row} | cut -d , -f 5`
  csproj_path_with_filename=`echo ${row} | cut -d , -f 6` # "SomeProject/SomeProject.csproj"

  # string up to trailing slash of csproj_path_with_filename
  csproj_path="${csproj_path_with_filename%/*}"

  # git checkout
  git checkout $commit_id

  # edit package.json
  sed -i -r -e "s#${old_repo_url}#${new_repo_url}#" $csproj_path_with_filename
  cat $csproj_path_with_filename

  # publish
  dotnet pack $csproj_path_with_filename --configuration Release
  dotnet nuget push "${csproj_path}/bin/Release/${package_name}.${version}.nupkg"  --api-key $pat --source "github-migrate"

  # restore changes
  git restore $csproj_path_with_filename
done < $csv_path
