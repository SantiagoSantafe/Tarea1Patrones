pipeline {
    agent any

    environment {
        REGISTRY = "nexus.146.190.187.99.nip.io"
        API_IMAGE_NAME = "mi-aplicacion-api"
        FRONTEND_IMAGE_NAME = "mi-aplicacion-frontend"
        IMAGE_TAG = "v${env.BUILD_NUMBER}"
        CHART_NAME = "chartpatrones"
        CHART_REPO = "helm-repo"
        DEPLOY_REPO = "git@github.com:SantiagoSantafe/manifestsPatrones.git"
        HELM_MANIFEST_PATH = "chartpatrones/values.yaml"
    }

    triggers {
        githubPush()
    }

    stages {
        stage('Checkout Código Fuente') {
            steps {
                git branch: 'main', url: 'https://github.com/SantiagoSantafe/Tarea1Patrones'
            }
        }

        stage('Checkout Manifests Repo') {
            steps {
                withCredentials([sshUserPrivateKey(credentialsId: 'github-deploy-key', keyFileVariable: 'SSH_KEY')]) {
                    script {
                        sh """
                            git clone ${DEPLOY_REPO} manifests
                        """
                    }
                }
            }
        }

        stage('Build Docker Images') {
            steps {
                script {
                    sh """
                        docker build -t ${REGISTRY}/${API_IMAGE_NAME}:${IMAGE_TAG} -f API/Dockerfile API
                        docker build -t ${REGISTRY}/${FRONTEND_IMAGE_NAME}:${IMAGE_TAG} -f FrontendK8s/Dockerfile FrontendK8s
                    """
                }
            }
        }

        stage('Login to Nexus') {
            steps {
                withCredentials([usernamePassword(credentialsId: 'nexus-repo-admin-credentials', usernameVariable: 'NEXUS_USER', passwordVariable: 'NEXUS_PASS')]) {
                    script {
                        sh "echo ${NEXUS_PASS} | docker login -u ${NEXUS_USER} --password-stdin ${REGISTRY}"
                    }
                }
            }
        }

        stage('Push Docker Images') {
            steps {
                script {
                    sh """
                        docker push ${REGISTRY}/${API_IMAGE_NAME}:${IMAGE_TAG}
                        docker push ${REGISTRY}/${FRONTEND_IMAGE_NAME}:${IMAGE_TAG}
                    """
                }
            }
        }

        stage('Actualizar Helm Values con yq') {
            steps {
                script {
                    sh """
                        cd manifests
                        yq eval -i '.api.image.tag = "${IMAGE_TAG}"' ${HELM_MANIFEST_PATH}
                        yq eval -i '.frontend.image.tag = "${IMAGE_TAG}"' ${HELM_MANIFEST_PATH}
                    """
                }
            }
        }

        stage('Push cambios en manifestsPatrones') {
            steps {
                withCredentials([sshUserPrivateKey(credentialsId: 'github-deploy-key', keyFileVariable: 'SSH_KEY')]) {
                    script {
                        sh """
                            cd manifests
                            git config user.email "jenkins@example.com"
                            git config user.name "Jenkins CI"
                            git add ${HELM_MANIFEST_PATH}
                            git commit -m "🔄 Actualización de imagen a ${IMAGE_TAG}"
                            GIT_SSH_COMMAND="ssh -i ${SSH_KEY} -o StrictHostKeyChecking=no" git push origin main
                        """
                    }
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
