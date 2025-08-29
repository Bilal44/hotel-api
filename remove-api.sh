# Stop and remove the running container
docker compose -f WaracleBooking/docker-compose.yml down

# Remove image
docker rmi waraclebooking-img
