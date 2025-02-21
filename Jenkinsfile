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
                    script {
                        if (fileExists('manifestsPatrones/.git')) {
                            sh """
                                cd manifestsPatrones
                                git fetch --all
                                git reset --hard origin/main
                                git pull
                            """
                        } else {
                            sh """
                                rm -rf manifestsPatrones || true
                                git clone https://${GIT_USER}:${GIT_PASS}@github.com/SantiagoSantafe/manifestsPatrones.git
                            """
                        }
                        
                        // Verifica si chartpatrones existe
                        sh "ls -la manifestsPatrones/chartpatrones || echo '❌ chartpatrones NO encontrado'"
                    }
                }
            }
        }

       stage('Empaquetar Helm Chart') {
    steps {
        script {
            sh """
                cd manifestsPatrones
                helm package ${CHART_NAME} --destination .
            """
        }
    }
}


      stage('Subir Helm Chart a Nexus') {
    steps {
        withCredentials([usernamePassword(credentialsId: 'nexus-repo-admin-credentials', usernameVariable: 'NEXUS_USER', passwordVariable: 'NEXUS_PASS')]) {
            script {
                sh '''
                    cd manifestsPatrones
                    curl -u $NEXUS_USER:$NEXUS_PASS --upload-file ${CHART_NAME}-*.tgz https://nexus.146.190.187.99.nip.io/repository/helm-repo/ -k || echo '❌ Error al subir Helm Chart'
                '''
            }
        }
    }
}

stage('Install Tools') {
    steps {
        sh '''
            # Install yq if not present in local directory
            if ! command -v ./yq &> /dev/null; then
                wget https://github.com/mikefarah/yq/releases/latest/download/yq_linux_amd64 -O yq
                chmod +x yq
            fi
        '''
    }
}

stage('Actualizar Helm Values con yq') {
    steps {
        script {
            sh """
                cd manifestsPatrones
                ../yq eval -i '.api.image.tag = "latest"' ${HELM_MANIFEST_PATH}
                ../yq eval -i '.frontend.image.tag = "latest"' ${HELM_MANIFEST_PATH}
            """
        }
    }
}

        stage('Push cambios en manifestsPatrones') {
    steps {
        withCredentials([usernamePassword(credentialsId: 'github-deploy-key', usernameVariable: 'GIT_USER', passwordVariable: 'GIT_PASS')]) {
            script {
                sh """
                    cd manifestsPatrones
                    git config user.email "jenkins@example.com"
                    git config user.name "Jenkins CI"
                    git add ${HELM_MANIFEST_PATH}
                    git commit -m "🔄 Actualización de Helm values" || echo "No changes to commit"
                    git push https://\${GIT_USER}:\${GIT_PASS}@github.com/SantiagoSantafe/manifestsPatrones.git main || echo '❌ Error al hacer push de cambios'
                """
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
