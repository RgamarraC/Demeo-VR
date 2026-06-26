using UnityEngine;
using UnityEditor;

namespace DemeoVR.Gameplay.Editor
{
    [CustomEditor(typeof(PieceData))]
    public class PieceDataEditor : UnityEditor.Editor
    {
        private SerializedProperty pieceType;
        
        // Estadísticas Base
        private SerializedProperty maxHealth;
        private SerializedProperty physicalDamage;
        private SerializedProperty magicDamage;
        
        // Mitigación
        private SerializedProperty armor;
        private SerializedProperty magicResistance;
        
        // Economía
        private SerializedProperty maxAP;
        
        // Progresión Héroes
        private SerializedProperty hpGrowth;
        private SerializedProperty physicalDamageGrowth;
        private SerializedProperty magicDamageGrowth;
        private SerializedProperty armorGrowth;
        private SerializedProperty magicResistanceGrowth;
        private SerializedProperty xpRequirements;
        
        // Recompensa Enemigos
        private SerializedProperty xpReward;

        private void OnEnable()
        {
            pieceType = serializedObject.FindProperty("pieceType");
            
            maxHealth = serializedObject.FindProperty("maxHealth");
            physicalDamage = serializedObject.FindProperty("physicalDamage");
            magicDamage = serializedObject.FindProperty("magicDamage");
            
            armor = serializedObject.FindProperty("armor");
            magicResistance = serializedObject.FindProperty("magicResistance");
            
            maxAP = serializedObject.FindProperty("maxAP");

            hpGrowth = serializedObject.FindProperty("hpGrowth");
            physicalDamageGrowth = serializedObject.FindProperty("physicalDamageGrowth");
            magicDamageGrowth = serializedObject.FindProperty("magicDamageGrowth");
            armorGrowth = serializedObject.FindProperty("armorGrowth");
            magicResistanceGrowth = serializedObject.FindProperty("magicResistanceGrowth");
            xpRequirements = serializedObject.FindProperty("xpRequirements");

            xpReward = serializedObject.FindProperty("xpReward");
        }

        public override void OnInspectorGUI()
        {
            // Actualiza el objeto serializado
            serializedObject.Update();

            // Dibujar Tipo de Pieza primero
            EditorGUILayout.PropertyField(pieceType);
            
            // Obtener el valor actual del enum
            PieceType currentType = (PieceType)pieceType.enumValueIndex;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Estadísticas Base", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(maxHealth);
            EditorGUILayout.PropertyField(physicalDamage);
            EditorGUILayout.PropertyField(magicDamage);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Mitigación de Daño", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(armor);
            EditorGUILayout.PropertyField(magicResistance);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Economía de Turno", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(maxAP);

            // === LÓGICA CONDICIONAL DE UI ===
            
            if (currentType == PieceType.Heroe1 || currentType == PieceType.Heroe2)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Sistema de Progresión (Héroes)", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(hpGrowth);
                EditorGUILayout.PropertyField(physicalDamageGrowth);
                EditorGUILayout.PropertyField(magicDamageGrowth);
                EditorGUILayout.PropertyField(armorGrowth);
                EditorGUILayout.PropertyField(magicResistanceGrowth);
                // El parámetro 'true' permite dibujar todo el arreglo expandible
                EditorGUILayout.PropertyField(xpRequirements, true); 
            }
            else if (currentType == PieceType.Enemigo || currentType == PieceType.Trampa)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Sistema de Recompensas (Enemigos)", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(xpReward);
            }

            // Aplica los cambios
            serializedObject.ApplyModifiedProperties();
        }
    }
}
