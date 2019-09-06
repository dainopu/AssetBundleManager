using UnityEngine ;
using UnityEngine.UI ;
using UnityEditor ;
using UnityEditor.UI ;

namespace uGUIHelper
{
	[CustomEditor(typeof(RichText), true)]
	[CanEditMultipleObjects]
	public class RichTextInspector : GraphicEditor
	{
		SerializedProperty m_Text ;
		SerializedProperty m_FontData ;
		
		protected override void OnEnable()
		{
			base.OnEnable() ;
			m_Text = serializedObject.FindProperty( "m_Text" ) ;
			m_FontData = serializedObject.FindProperty( "m_FontData" ) ;
		}
		
		public override void OnInspectorGUI()
		{
			serializedObject.Update() ;
			
			EditorGUILayout.PropertyField( m_Text ) ;
			EditorGUILayout.PropertyField( m_FontData ) ;
			AppearanceControlsGUI() ;
			RaycastControlsGUI() ;
			serializedObject.ApplyModifiedProperties() ;
			
			//----------------------------------------------------------

			EditorGUILayout.Separator() ;   // ������؂�X�y�[�X
			
			// �^�[�Q�b�g�̃C���X�^���X
			RichText tTarget = target as RichText ;
			
			//----------------------------------------------------------

			bool tViewControllEnabled = EditorGUILayout.Toggle( "View Controll Enabled", tTarget.viewControllEnabled ) ;
			if( tViewControllEnabled != tTarget.viewControllEnabled )
			{
				Undo.RecordObject( tTarget, "RichText : View Controll Enabled Change" ) ;	// �A���h�E�o�b�t�@�ɓo�^
				tTarget.viewControllEnabled = tViewControllEnabled ;
				EditorUtility.SetDirty( tTarget ) ;
//				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
			}

			//----------------------------------

			if( tTarget.viewControllEnabled == true )
			{
				int tLengthOfView = EditorGUILayout.IntSlider( "Length Of View", tTarget.lengthOfView, -1, tTarget.length ) ;
				if( tLengthOfView != tTarget.lengthOfView )
				{
					Undo.RecordObject( tTarget, "RichText : Length Of View Change" ) ;	// �A���h�E�o�b�t�@�ɓo�^
					tTarget.lengthOfView = tLengthOfView ;
					EditorUtility.SetDirty( tTarget ) ;
				}

				int tLine  = tTarget.line ;

				int tStartLineOfView = EditorGUILayout.IntSlider( "Start Line Of View", tTarget.startLineOfView, 0, tLine - 1 ) ;

				if( tStartLineOfView >  ( tLine - 1 ) )
				{
					tStartLineOfView  = ( tLine - 1 ) ;
				}

				if( tStartLineOfView != tTarget.startLineOfView )
				{
					tTarget.startLineOfView = tStartLineOfView ;
					EditorUtility.SetDirty( tTarget ) ;
				}

				int tEndLineOfView = EditorGUILayout.IntSlider( "End Line Of View", tTarget.endLineOfView, tTarget.startLineOfView + 1, tLine ) ;

				if( tEndLineOfView <  ( tTarget.startLineOfView + 1 ) )
				{
					tEndLineOfView  = ( tTarget.startLineOfView + 1 ) ;
				}

				if( tEndLineOfView >  tLine )
				{
					tEndLineOfView  = tLine ;
				}

				if( tEndLineOfView != tTarget.endLineOfView )
				{
					tTarget.endLineOfView = tEndLineOfView ;
					EditorUtility.SetDirty( tTarget ) ;
				}

				//----------------------------------------------------------

				int tStartOffsetOfView	= tTarget.startOffsetOfView ;
				int tEndOffsetOfView	= tTarget.endOffsetOfView ;

				int tStartOffsetOfFade = EditorGUILayout.IntSlider( "Start Offset Of Fade", tTarget.startOffsetOfFade, tStartOffsetOfView, tEndOffsetOfView ) ;

				if( tStartOffsetOfFade <  tStartOffsetOfView )
				{
					tStartOffsetOfFade  = tStartOffsetOfView ;
				}

				if( tStartOffsetOfFade >  tEndOffsetOfView )
				{
					tStartOffsetOfFade  = tEndOffsetOfView ;
				}

				if( tStartOffsetOfFade != tTarget.startOffsetOfFade )
				{
					Undo.RecordObject( tTarget, "RichText : Start Offset Of Fade Change" ) ;	// �A���h�E�o�b�t�@�ɓo�^
					tTarget.startOffsetOfFade = tStartOffsetOfFade ;
					EditorUtility.SetDirty( tTarget ) ;
				}

				int tEndOffsetOfFade = EditorGUILayout.IntSlider( "End Offset Of Fade", tTarget.endOffsetOfFade, tTarget.startOffsetOfFade, tEndOffsetOfView  ) ;

				if( tEndOffsetOfFade <  tTarget.startOffsetOfFade )
				{
					tEndOffsetOfFade  = tTarget.startOffsetOfFade ;
				}

				if( tEndOffsetOfFade >  tEndOffsetOfView )
				{
					tEndOffsetOfFade  = tEndOffsetOfView ;
				}

				if( tEndOffsetOfFade != tTarget.endOffsetOfFade )
				{
					Undo.RecordObject( tTarget, "RichText : End Offset Of Fade Change" ) ;	// �A���h�E�o�b�t�@�ɓo�^
					tTarget.endOffsetOfFade = tEndOffsetOfFade ;
					EditorUtility.SetDirty( tTarget ) ;
				}


				float tRatioOfFade = EditorGUILayout.Slider( "Ratio Of Fade", tTarget.ratioOfFade, 0.0f, 1.0f ) ;
				if( tRatioOfFade != tTarget.ratioOfFade )
				{
					Undo.RecordObject( tTarget, "RichText : Ratio Of Fade Change" ) ;	// �A���h�E�o�b�t�@�ɓo�^
					tTarget.ratioOfFade = tRatioOfFade ;
					EditorUtility.SetDirty( tTarget ) ;
				}

				int tWidthOfFade = EditorGUILayout.IntSlider( "Width Of Fade", tTarget.widthOfFade, 0, 10 ) ;
				if( tWidthOfFade != tTarget.widthOfFade )
				{
					Undo.RecordObject( tTarget, "RichText : Width Of Fade Change" ) ;	// �A���h�E�o�b�t�@�ɓo�^
					tTarget.widthOfFade = tWidthOfFade ;
					EditorUtility.SetDirty( tTarget ) ;
				}
			}

			//----------------------------------------------------------

			EditorGUILayout.Separator() ;   // ������؂�X�y�[�X
			
			float tRubySizeScale = EditorGUILayout.Slider( "Ruby Size Scale", tTarget.rubySizeScale, 0.1f, 1.0f ) ;
			if( tRubySizeScale != tTarget.rubySizeScale )
			{
				Undo.RecordObject( tTarget, "RichText : Ruby Size Scale Change" ) ;	// �A���h�E�o�b�t�@�ɓo�^
				tTarget.rubySizeScale = tRubySizeScale ;
				EditorUtility.SetDirty( tTarget ) ;
			}

			float tSupOrSubSizeScale = EditorGUILayout.Slider( "Sup Or Sub Size Scale", tTarget.supOrSubSizeScale, 0.1f, 1.0f ) ;
			if( tSupOrSubSizeScale != tTarget.supOrSubSizeScale )
			{
				Undo.RecordObject( tTarget, "RichText : Sup Or Sub Size Scale Change" ) ;	// �A���h�E�o�b�t�@�ɓo�^
				tTarget.supOrSubSizeScale = tSupOrSubSizeScale ;
				EditorUtility.SetDirty( tTarget ) ;
			}

			float tTopMarginSpacing = EditorGUILayout.FloatField( "Top Margin Spacing", tTarget.topMarginSpacing ) ;
			if( tTopMarginSpacing != tTarget.topMarginSpacing )
			{
				Undo.RecordObject( tTarget, "RichText : Top Margin Spacing Change" ) ;	// �A���h�E�o�b�t�@�ɓo�^
				tTarget.topMarginSpacing = tTopMarginSpacing ;
				EditorUtility.SetDirty( target ) ;
			}

			float tBottomMarginSpacing = EditorGUILayout.FloatField( "Bottom Margin Spacing", tTarget.bottomMarginSpacing ) ;
			if( tBottomMarginSpacing != tTarget.bottomMarginSpacing )
			{
				Undo.RecordObject( tTarget, "RichText : Bottom Margin Spacing Change" ) ;	// �A���h�E�o�b�t�@�ɓo�^
				tTarget.bottomMarginSpacing = tBottomMarginSpacing ;
				EditorUtility.SetDirty( target ) ;
			}
		}
	}
}


