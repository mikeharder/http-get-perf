FROM maven:3-jdk-11 as build

# Copy pom.xml and install dependencies
WORKDIR /app
COPY pom.xml .
RUN mvn dependency:go-offline

# Copy sources and build JAR
COPY . .
RUN mvn -o package

FROM openjdk:11-jre AS runtime
WORKDIR /app
COPY --from=build /app/target/java-reactor-netty-1.0-SNAPSHOT-jar-with-dependencies.jar .
ENTRYPOINT ["java", "-jar", "java-reactor-netty-1.0-SNAPSHOT-jar-with-dependencies.jar"]
