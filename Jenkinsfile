pipeline {
    agent any

    environment {
        REGISTRY = "nexus.146.190.187.99.nip.io"
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
                withCredentials([usernamePassword(credentialsId: 'github-deploy-key', usernameVariable: 'GIT_USER', passwordVariable: 'GIT_PASS')]) {
                    sh "git clone https://${GIT_USER}:${GIT_PASS}@github.com/SantiagoSantafe/manifestsPatrones.git"
                }
            }
        }

        stage('Empaquetar Helm Chart') {
            steps {
                script {
                    sh "helm package ${CHART_NAME} --destination ."
                }
            }
        }

        stage('Subir Helm Chart a Nexus') {
            steps {
                withCredentials([usernamePassword(credentialsId: 'nexus-repo-admin-credentials', usernameVariable: 'NEXUS_USER', passwordVariable: 'NEXUS_PASS')]) {
                    script {
                        sh """
                            helm registry login -u ${NEXUS_USER} -p ${NEXUS_PASS} ${REGISTRY}
                            helm push ${CHART_NAME}-*.tgz oci://${REGISTRY}/repository/${CHART_REPO}
                        """
                    }
                }
            }
        }

        stage('Actualizar Helm Values con yq') {
            steps {
                script {
                    sh """
                        cd manifestsPatrones
                        yq eval -i '.api.image.tag = "latest"' ${HELM_MANIFEST_PATH}
                        yq eval -i '.frontend.image.tag = "latest"' ${HELM_MANIFEST_PATH}
                    """
                }
            }
        }

        stage('Push cambios en manifestsPatrones') {
            steps {
                withCredentials([sshUserPrivateKey(credentialsId: 'github-deploy-key', keyFileVariable: 'SSH_KEY')]) {
                    script {
                        sh """
                            cd manifestsPatrones
                            git config user.email "jenkins@example.com"
                            git config user.name "Jenkins CI"
                            git add ${HELM_MANIFEST_PATH}
                            git commit -m "🔄 Actualización de Helm values"
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
