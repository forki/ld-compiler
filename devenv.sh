APP_DIR=${APP_DIR:?"Need to set APP_DIR"}
docker run --rm -it --name ldcompiler_devenv -v $APP_DIR:/app -w "/app" nice/ld-publisher-base bash  
