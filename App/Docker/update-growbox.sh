#! /bin/bash -e
docker compose down 
docker image remove emmuss/growbox -f
docker compose up -d