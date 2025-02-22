pipeline {
    agent any

    environment {
        // ** REGISTRO DE IMÁGENES CAMBIADO A DOCKER HUB **
        REGISTRY = "docker.io"        // Registro de Docker Hub (público en este ejemplo)
        CHART_NAME = "chartpatrones"
        CHART_REPO = "helm-repo"       // Ya no se usa para imágenes, solo para el chart en Nexus (opcional)
        DEPLOY_REPO = "git@github.com:SantiagoSantafe/manifestsPatrones.git"
        HELM_MANIFEST_PATH = "chartpatrones/values.yaml"
        // ** Nombres de imágenes en Docker Hub - AMBAS IMÁGENES AHORA BAJO 'santiagosantafe' **
        API_IMAGE_NAME = "${REGISTRY}/santiagosantafe/api-app"      // **CORREGIDO: API ahora también a santiagosantafe**
        FRONTEND_IMAGE_NAME = "${REGISTRY}/santiagosantafe/tareak8s"
        IMAGE_TAG = "${env.GIT_COMMIT.substring(0, 7)}" // Tag dinámico basado en Commit SHA (ejemplo)
        // ** CREDENCIALES DE DOCKER HUB - USANDO SOLO LAS DE 'santafe' PARA AMBAS IMÁGENES **
        DOCKERHUB_CREDENTIALS_SANTAFE_ID = 'ss-dockerhub-token'   // ID de credenciales de Jenkins para AMBAS imágenes (usuario santafe) - **CORREGIDO: Usando solo las credenciales de 'santafe'**
    }

    triggers {
        githubPush() // Disparar el pipeline con cada push a GitHub (repo de código fuente: Tarea1Patrones)
    }

    stages {
        stage('Checkout Código Fuente') {
            steps {
                git branch: 'main', url: 'https://github.com/SantiagoSantafe/Tarea1Patrones' // Repo de código fuente de la aplicación
            }
        }

        stage('Docker Login') { // **STAGE MODIFICADO: Docker Login -  Usando credenciales 'santafe' para AMBAS**
            steps {
                script {
                    withCredentials([usernamePassword(credentialsId: "${DOCKERHUB_CREDENTIALS_SANTAFE_ID}", usernameVariable: 'DOCKER_USER', passwordVariable: 'DOCKER_PASS')]) {
                        sh 'docker login -u $DOCKER_USER -p $DOCKER_PASS' // Login a Docker Hub con credenciales de 'santafe'
                    }
                }
            }
        }

        stage('Build and Push Docker Images API') { // **STAGE MODIFICADO: Build y Push de la API -  A santiagosantafe**
            steps {
                script {
                    // ** Construir y Pushear imagen de la API a Docker Hub **
                    sh "docker build -t ${API_IMAGE_NAME}:${IMAGE_TAG} ./API" // Contexto correcto: ./API
                    sh "docker push ${API_IMAGE_NAME}:${IMAGE_TAG}"
                    echo "✅ Imagen Docker de la API pushada a Docker Hub (usuario santiagosantafe) con tag: ${IMAGE_TAG}" // **CORREGIDO: Mensaje a santiagosantafe**
                }
            }
        }

        stage('Build and Push Docker Images Frontend') { // **STAGE MODIFICADO: Build y Push del Frontend - A santiagosantafe**
            steps {
                script {
                    // ** Construir y Pushear imagen del Frontend a Docker Hub **
                    sh "docker build -t ${FRONTEND_IMAGE_NAME}:${IMAGE_TAG} ./FrontendK8s" // Contexto correcto: ./FrontendK8s
                    sh "docker push ${FRONTEND_IMAGE_NAME}:${IMAGE_TAG}"
                    echo "✅ Imagen Docker del Frontend pushada a Docker Hub (usuario santiagosantafe) con tag: ${IMAGE_TAG}" // **CORREGIDO: Mensaje a santiagosantafe**
                }
            }
        }

        stage('Docker Logout') { // **STAGE MODIFICADO: Docker Logout -  Logout general (ya que usamos las mismas credenciales)**
            steps {
                script {
                    sh "docker logout docker.io" // Logout de Docker Hub
                }
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
                        // Verifica si chartpatrones existe (opcional para debug)
                        sh "ls -la manifestsPatrones/chartpatrones || echo '❌ chartpatrones NO encontrado'"
                    }
                }
            }
        }

        stage('Install Tools') {
            steps {
                sh '''
                    # Install yq si no está presente (para manipular YAML)
                    if (! command -v ./yq &> /dev/null) then
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
                        ../yq eval -i '.api.image.tag = "${IMAGE_TAG}"' ${HELM_MANIFEST_PATH}  // Actualizar tag de imagen de la API en values.yaml
                        ../yq eval -i '.frontend.image.tag = "${IMAGE_TAG}"' ${HELM_MANIFEST_PATH} // Actualizar tag de imagen del Frontend en values.yaml
                    """
                }
            }
        }

        stage('Empaquetar Helm Chart') {
            steps {
                script {
                    sh """
                        cd manifestsPatrones
                        helm package ${CHART_NAME} --destination . // Empaquetar el chart en .tgz
                    """
                }
            }
        }

        stage('Subir Helm Chart a Nexus (Opcional)') { // **STAGE OPCIONAL - Subida a Nexus ahora es manual/ocasional**
            steps {
                withCredentials([usernamePassword(credentialsId: 'nexus-repo-admin-credentials', usernameVariable: 'NEXUS_USER', passwordVariable: 'NEXUS_PASS')]) {
                    script {
                        // ** Subida a Nexus COMENTADA - Desactivada en este flujo simplificado **
                        // sh '''
                        //     cd manifestsPatrones
                        //     curl -u $NEXUS_USER:$NEXUS_PASS --upload-file ${CHART_NAME}-*.tgz https://nexus.146.190.187.99.nip.io/repository/helm-repo/ -k || echo '❌ Error al subir Helm Chart'
                        // '''
                        echo "Subida automática del Helm Chart a Nexus OMITIDA en este pipeline (Docker Hub para imágenes)"
                        echo "Para subir el chart manualmente a Nexus, descomenta las líneas en este stage."
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
                            git add ${HELM_MANIFEST_PATH} chartpatrones/*.tgz # Añadir values.yaml y el chart empaquetado (si se sube a Nexus)
                            git commit -m "🔄 Actualización de Helm values e imagen a Docker Hub: ${IMAGE_TAG}" || echo "No changes to commit"
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
