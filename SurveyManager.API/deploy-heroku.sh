docker build -t ashaheen97/survey-manager-v1 .
docker tag ashaheen97/survey-manager-v1 registry.heroku.com/survey-manager-v1/web
docker push registry.heroku.com/survey-manager-v1/web
heroku container:release web -a survey-manager-v1
