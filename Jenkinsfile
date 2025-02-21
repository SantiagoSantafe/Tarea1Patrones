pipeline {
    agent any

    environment {
        REGISTRY = "nexus.146.190.187.99.nip.io"
        IMAGE_NAME = "mi-aplicacion"
        CHART_NAME = "chartpatrones"
        CHART_REPO = "helm-repo"
    }

    triggers {
        githubPush()
    }

    stages {
        stage('Checkout') {
    steps {
        git branch: 'main', url: 'https://github.com/SantiagoSantafe/Tarea1Patrones'
    }
}

        stage('Build Docker Image') {
            steps {
                script {
                    def imageTag = "v${env.BUILD_NUMBER}"
                    sh "docker build -t ${REGISTRY}/${IMAGE_NAME}:${imageTag} ."
                }
            }
        }

        stage('Login to Nexus') {
            steps {
                // Usamos withCredentials para obtener las credenciales de Jenkins
                withCredentials([usernamePassword(credentialsId: 'nexus-credentials', usernameVariable: 'NEXUS_USER', passwordVariable: 'NEXUS_PASS')]) {
                    script {
                        sh "echo ${NEXUS_PASS} | docker login -u ${NEXUS_USER} --password-stdin ${REGISTRY}"
                    }
                }
            }
        }

        stage('Push Docker Image') {
            steps {
                withCredentials([usernamePassword(credentialsId: 'nexus-credentials', usernameVariable: 'NEXUS_USER', passwordVariable: 'NEXUS_PASS')]) {
                    script {
                        def imageTag = "v${env.BUILD_NUMBER}"
                        sh "docker push ${REGISTRY}/${IMAGE_NAME}:${imageTag}"
                    }
                }
            }
        }

        stage('Package Helm Chart') {
            steps {
                script {
                    sh "helm package charts/${CHART_NAME}"
                }
            }
        }

        stage('Push Helm Chart to Nexus') {
            steps {
                // Usamos withCredentials de nuevo para el push del chart
                withCredentials([usernamePassword(credentialsId: 'nexus-credentials', usernameVariable: 'NEXUS_USER', passwordVariable: 'NEXUS_PASS')]) {
                    script {
                        def chartFile = sh(script: "ls ${CHART_NAME}-*.tgz", returnStdout: true).trim()
                        sh "helm push ${chartFile} oci://${REGISTRY}/${CHART_REPO} --username ${NEXUS_USER} --password ${NEXUS_PASS}"
                    }
                }
            }
        }

        stage('Update Helm Repo') {
            steps {
                script {
                    sh "helm repo update"
                }
            }
        }
    }

    post {
        success {
            echo "✅ Pipeline completed successfully!"
        }
        failure {
            echo "❌ Pipeline failed!"
        }
    }
}
