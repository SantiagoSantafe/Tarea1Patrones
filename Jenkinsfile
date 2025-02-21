pipeline {
    agent any

    environment {
        REGISTRY = "nexus.146.190.187.99.nip.io"
        API_IMAGE_NAME = "mi-aplicacion-api"
        FRONTEND_IMAGE_NAME = "mi-aplicacion-frontend"
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

        stage('Build Docker Images') {
            steps {
                script {
                    def imageTag = "v${env.BUILD_NUMBER}"
                    
                    // Construir imagen para API
                    sh "docker build -t ${REGISTRY}/${API_IMAGE_NAME}:${imageTag} -f API/Dockerfile API"
                    
                    // Construir imagen para Frontend
                    sh "docker build -t ${REGISTRY}/${FRONTEND_IMAGE_NAME}:${imageTag} -f FrontendK8s/Dockerfile FrontendK8s"
                }
            }
        }

        stage('Login to Nexus') {
            steps {
                // Se usa el ID correcto de las credenciales
                withCredentials([usernamePassword(credentialsId: 'nexus-repo-admin-credentials', usernameVariable: 'NEXUS_USER', passwordVariable: 'NEXUS_PASS')]) {
                    script {
                        sh "echo ${NEXUS_PASS} | docker login -u ${NEXUS_USER} --password-stdin ${REGISTRY}"
                    }
                }
            }
        }

        stage('Push Docker Images') {
            steps {
                withCredentials([usernamePassword(credentialsId: 'nexus-repo-admin-credentials', usernameVariable: 'NEXUS_USER', passwordVariable: 'NEXUS_PASS')]) {
                    script {
                        def imageTag = "v${env.BUILD_NUMBER}"
                        sh "docker push ${REGISTRY}/${API_IMAGE_NAME}:${imageTag}"
                        sh "docker push ${REGISTRY}/${FRONTEND_IMAGE_NAME}:${imageTag}"
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
                withCredentials([usernamePassword(credentialsId: 'nexus-repo-admin-credentials', usernameVariable: 'NEXUS_USER', passwordVariable: 'NEXUS_PASS')]) {
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
