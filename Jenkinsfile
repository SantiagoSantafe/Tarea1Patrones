pipeline {
    agent any

    environment {
        REGISTRY = "docker.io"
        CHART_NAME = "chartpatrones"
        CHART_REPO = "helm-repo"
        DEPLOY_REPO = "git@github.com:SantiagoSantafe/manifestsPatrones.git"
        HELM_MANIFEST_PATH = "chartpatrones/values.yaml"
        API_IMAGE_NAME = "${REGISTRY}/santiagosantafe/api-app"
        FRONTEND_IMAGE_NAME = "${REGISTRY}/santiagosantafe/tareak8s"
        DOCKERHUB_CREDENTIALS_SANTAFE_ID = 'ss-dockerhub-token'
        NEXUS_HELM_REPO_URL = "https://nexus.146.190.187.99.nip.io/repository/helm-repo/"
        CHART_VERSION = "0.1.${BUILD_NUMBER}"
    }

    triggers {
        githubPush()
    }

    options {
        skipDefaultCheckout()
    }

    stages {
        stage('Cleanup Workspace') {
            steps {
                cleanWs()
            }
        }

        stage('Checkout Código Fuente') {
            steps {
                script {
                    // Checkout code first
                    git branch: 'main', url: 'https://github.com/SantiagoSantafe/Tarea1Patrones'
                    // Then set IMAGE_TAG based on the commit
                    env.IMAGE_TAG = sh(script: 'git rev-parse --short HEAD', returnStdout: true).trim()
                }
            }
        }

        stage('Docker Login') {
            steps {
                script {
                    withCredentials([usernamePassword(credentialsId: "${DOCKERHUB_CREDENTIALS_SANTAFE_ID}", usernameVariable: 'DOCKER_USER', passwordVariable: 'DOCKER_PASS')]) {
                        sh 'docker login -u $DOCKER_USER -p $DOCKER_PASS'
                    }
                }
            }
        }

        stage('Build and Push Docker Images API') {
            steps {
                script {
                    sh "docker build -t ${API_IMAGE_NAME}:${IMAGE_TAG} ./API"
                    sh "docker push ${API_IMAGE_NAME}:${IMAGE_TAG}"
                    echo "✅ Imagen Docker de la API pushada a Docker Hub (usuario santiagosantafe) con tag: ${IMAGE_TAG}"
                }
            }
        }

        stage('Build and Push Docker Images Frontend') {
            steps {
                script {
                    sh "docker build -t ${FRONTEND_IMAGE_NAME}:${IMAGE_TAG} ./FrontendK8s"
                    sh "docker push ${FRONTEND_IMAGE_NAME}:${IMAGE_TAG}"
                    echo "✅ Imagen Docker del Frontend pushada a Docker Hub (usuario santiagosantafe) con tag: ${IMAGE_TAG}"
                }
            }
        }

        stage('Docker Logout') {
            steps {
                script {
                    sh "docker logout docker.io"
                }
            }
        }

        stage('Checkout Manifests Repo') {
            steps {
                withCredentials([usernamePassword(credentialsId: 'github-deploy-key', usernameVariable: 'GIT_USER', passwordVariable: 'GIT_PASS')]) {
                    script {
                        if (fileExists('manifestsPatrones/.git')) {
                            sh '''
                                cd manifestsPatrones
                                git fetch --all
                                git reset --hard origin/main
                                # git pull
                                echo "i  Manifests Repo: Después de fetch y reset:"
                                git log --oneline -n 5
                                git status
                                ls -la chartpatrones
                            '''
                        } else {
                            sh '''
                                rm -rf manifestsPatrones || true
                                git clone https://${GIT_USER}:${GIT_PASS}@github.com/SantiagoSantafe/manifestsPatrones.git
                                cd manifestsPatrones
                                ls -la chartpatrones || echo '❌ chartpatrones NO encontrado'
                                ls -la
                                echo "i Manifests Repo: Después de clonar:"
                                git log --oneline -n 5
                                git status
                                ls -la chartpatrones
                            '''
                        }
                    }
                }
            }
        }

        stage('Install Tools') {
            steps {
                sh '''
                    cd manifestsPatrones
                    # Install yq if not present
                    if (! command -v ../yq &> /dev/null) then
                        wget https://github.com/mikefarah/yq/releases/latest/download/yq_linux_amd64 -O ../yq
                        chmod +x ../yq
                    fi

                    # **DESCARGAR HELM BINARIO LOCALMENTE EN WORKSPACE**
                    if [ ! -f ./helm ]; then
                        echo "Descargando Helm CLI..."
                        HELM_VERSION="v3.14.0" # Puedes usar la versión más reciente
                        HELM_DOWNLOAD_URL="https://get.helm.sh/helm-\${HELM_VERSION}-linux-amd64.tar.gz"
                        curl -sSLf "\${HELM_DOWNLOAD_URL}" | tar -xz
                        mv linux-amd64/helm ./helm
                        chmod +x ./helm
                        echo "Helm CLI descargado y disponible en ./helm"
                    else
                        echo "Helm CLI ya existe en el workspace."
                    fi

                    # **VERIFY HELM VERSION**
                    echo "Helm Version:"
                    ./helm version --short # Print short version info
                '''
            }
        }


        stage('Actualizar Helm Values con yq') {
            steps {
                script {
                    sh """
                        cd manifestsPatrones
                        # **CORRECTED PATH:** Using relative path 'chartpatrones/values.yaml'
                        ../yq eval -i '.api.image.tag = "${IMAGE_TAG}"' chartpatrones/values.yaml
                        ../yq eval -i '.frontend.image.tag = "${IMAGE_TAG}"' chartpatrones/values.yaml
                    """
                }
            }
        }

        // **DEBUG: Output values.yaml content after yq update**
        stage('DEBUG: Verify values.yaml Update') {
            steps {
                sh 'cat manifestsPatrones/chartpatrones/values.yaml'
            }
        }


        stage('Empaquetar Helm Chart') {
            steps {
                script {
                    sh """
                        cd manifestsPatrones
                        # **USANDO HELM DESCARGADO LOCALMENTE:  ./helm package ...**
                        ./helm package ${CHART_NAME} --version ${CHART_VERSION} --destination .
                        echo "📦 Helm Chart empaquetado con versión: ${CHART_VERSION}"
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

                            HELM_CHART_FILE=$(find . -name "*.tgz" -print | head -n 1)
                            if [ -z "${HELM_CHART_FILE}" ]; then
                                echo "❌ No se encontró el archivo del Helm Chart empaquetado (*.tgz)"
                                exit 1
                            fi
                
                            echo "📦 Subiendo Helm Chart versión ${CHART_VERSION}: ${HELM_CHART_FILE} a Nexus..."
                
                            # Verificar archivo
                            if [ ! -f "${HELM_CHART_FILE}" ]; then
                                echo "❌ ERROR: El archivo ${HELM_CHART_FILE} no existe!"
                                exit 1
                            fi
                
                            # **Autenticar con Helm Registry Login - INSECURE: --insecure-skip-tls-verify**
                            echo "🔐 Autenticando con helm registry login (INSECURE --insecure-skip-tls-verify)..."
                            ./helm registry login ${NEXUS_HELM_REPO_URL} --username "${NEXUS_USER}" --password "${NEXUS_PASS}" --insecure-skip-tls-verify
                            if [ $? -ne 0 ]; then
                                echo "❌ Error en helm registry login. Fallando pipeline."
                                exit 1
                            fi
                            echo "✅ Autenticación exitosa con helm registry login (INSECURE --insecure-skip-tls-verify)."


                            # **Subir Helm Chart (sin flags de usuario/contraseña)**
                            echo "🚀 Pushing Helm chart usando ./helm push..."
                            ./helm push "${HELM_CHART_FILE}" ${NEXUS_HELM_REPO_URL}
                            if [ $? -eq 0 ]; then
                                echo "✅ Helm Chart subido exitosamente a Nexus: ${NEXUS_HELM_REPO_URL}"
                            else
                                echo "❌ Error al subir el Helm Chart a Nexus usando ./helm push"
                                exit 1
                            fi
                        '''
                    }
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
                            git add ${HELM_MANIFEST_PATH} chartpatrones/*.tgz
                            git commit -m "🔄 Actualización de Helm values e imagen a Docker Hub: ${IMAGE_TAG} y Helm Chart versión ${CHART_VERSION}" || echo "No changes to commit"
                            git push https://\${GIT_USER}:\${GIT_PASS}@github.com/SantiagoSantafe/manifestsPatrones.git main || echo '❌ Error al hacer push de cambios'
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