   M S B A M L   `   `   `  ����?  ;PYAC, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'clr-namespace:PYAC.Views
PYAC.Views  5	          PYAC.Views.RecipePage   5      R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      P LPrism.Wpf, Version=6.3.0.0, Culture=neutral, PublicKeyToken=40ee6c3a2184dc59$prismhttp://prismlibrary.com/  5      $localclr-namespace:PYAC.Views  5	      !  Prism.Mvvm.ViewModelLocator    AutoWireViewModel  .  � 	Resources 5         RefreshImage��%�   &	        �� 5   
   $��Uniform=�6%   $T��NC:\Users\devdd\Desktop\IMPORTANT\PYAC_VERSIONS\V2\PYAC\PYAC\Media\refresh2.png��67   5      .��5      �� ���#EFEFEF���#d94449$	��0.5q�68    �� CornerRadius$ 10,10,10,10��6N   $��0q�6i    �� Padding$ 5q�6u   ._�5   
   �  � ColumnDefinitions 5      �� 5      $
��0.5* �6#   �� 5      $��2* �6#    � RowDefinitions 5      �� 5      $��* �6    �� 5      $��5* �6    �� 5      $
��0.5* �6    .+�5      �� $��1��6   $��0��6"   
����������#dd565b$��2q�6[   $ 10,10,10,10��6o   $��5q�6�   �� 5      $��1��6   $��2��6"   
����������#dd565b$��2q�6[   $ 10,10,10,10��6o   $��5q�6�   �� 5#      $��0��6   $��0��6&    �� Orientation$ Vertical=�66   .+�5$      �� $��Recipes��6   $��30(�6+   $
��Bold"�69   $��Center=�6K   $��Center=�6h   $��9q�6�   �� 5%      $ 
Horizontal=�6   $��Center=�66   .+�5&      �� $��20(�6Q   $
��Bold"�6_   $��Center=�6q   $��Center=�6�   $��0q�6�   ��6    �� *SelectedRecipeName+ �� Mode$ OneWay=�6�   �� 5'      $	�� | ��6    $��20(�6+   $
��Bold"�69   $��Center=�6K   $��Center=�6h   $��0q�6�   �� 5(      $��20(�6S   $
��Bold"�6a   $��Center=�6s   $��Center=�6�   $��0q�6�   ��6    �� *SelectedRecipeNumber+$ OneWay=�6�   �� 5)      $��20(�6O   ���#EFEFEF$
��Bold"�6r   $��Center=�6�   $��Center=�6�   $��0q�6�   ��6    �� *SelectedRecipeID+$ OneWay=�6�   5*      5+      �� 5-      $��0��6   $��1��6%   $��Segments��65   $��30(�6E   $
��Bold"�6S   $��Center=�6e   $��Center=�6�   $��
-120,0,0,0q�6�   &   System.Windows.Controls.DataGrid  5/      -   5        $��dataGridRecipes��5/         CanUserAddRows . 	   
IsReadOnly	 .  $��1��6W   $��0��6d   
   AutoGenerateColumns
 .  $��15q�6�   $��15(�6�   $��Verdana)�6  ��6�   �� *	Recipes+ �� SelectedItem 6�   ��  �� Path$ CurrentSelectedRecipe�$ TwoWay=�   Columns 52      0  *System.Windows.Controls.DataGridTextColumn  53      ,  &System.Windows.Controls.DataGridColumn   Header$ ID��6)   1  +System.Windows.Controls.DataGridBoundColumn   Binding 65   �� *	Recipe_ID+  54      $
 Name��6)    67   �� *Recipe_Name+  55      $ Number��6)    69   �� *Recipe_Number+   ColumnHeaderStyle 57      �� 58       �� 
TargetType=  7System.Windows.Controls.Primitives.DataGridColumnHeader"  6     MetroDataGridColumnHeader �� BasedOn# [ 6G   .�59      ��  �� Property � �� �� Value$ Center=�6G   5:      5T        5^      -   5        $��dataGridSegments��5^      $��1��6/   $��1��6<   
 .  $��15q�6�   $��15(�6�   $��Verdana)�6�   ��6h   �� *
Segments+ 6�   �� $ CurrentSelectedSegment�$ TwoWay=� 5_        5`      $ ID��6)    6@   �� *ID+  5a      $ Number��6)    6@   �� *Segment_Number+  5b      $ Ramp SP��6)    6@   �� *	RAMP_SP+  5c      $ Min Ramp��6)    6@   �� *
MIN_RAMP+  5d      $ Max Ramp��6)    6@   �� *
MAX_RAMP+  5e      $ Soak SP ��6)    6@   �� *	SOAK_SP+  5f      $ Min Soak��6)    6@   �� *
MIN_SOAK+  5g      $ Max Soak��6)    6@   �� *
MAX_SOAK+  5h      $ 	Soak Time��6)    6@   �� *	SOAK_TIME+  5i      $ Low Temp SP��6)    6@   �� *LOW_TEMP_MODE_SP+  5j      $ Alarm Temp TH��6)    6@   �� *ALARM_TEMP_TH+  5k      $ Low Temp Mode (Y/N)��6)    6F   �� *LOW_TEMP_MODE_EN+ 5m      �� 5n      "  6   # [ 6G   .�5o      ��  � ��$ Center=�6G   5p      5r      � 5t      $��2��6   $��0��6     5v      �� 5w      $��* �6'   �� 5x      $��* �6'   �� 5y      $��* �6'   .+�5}      �� $��0��6   #��[  6)   
����������#dd565b$��2q�6�   $��5q�6�   ��6Q   �� *RefreshRecipesCommand+6�   �� 5~      $��1��6   $��+��6)   $��0q�65   $��25(�6A   
����������#dd565b$��2q�6x   $��5q�6�   ��6�   �� *NewRecipeCommand+�� 5      $��2��6   $��-��6)   $��0q�65   $��25(�6A   
����������#dd565b$��2q�6x   $��5q�6�   ��6�   �� *DeleteRecipeCommand+5�      � 5�      $��2��6   $��1��6     5�      �� 5�      $��* �6'   �� 5�      $��* �6'   �� 5�      $��* �6'   .+�5�      �� $��0��6   $��Add Segments��6)   
����������#dd565b$��2q�6i   $��5q�6}   ��6�   �� *AddSegmentCommand+��6�   �� *ItemsSource+ �� ElementName$ dataGridSegments���� 5�      $��1��6   $��Remove Selected Segment��6)   
����������#dd565b$��2q�6t   $��5q�6�   ��6�   �� *RemoveSegmentCommand+�� 5�      $��2��6   $��Save Segment Changes��6)   
����������#dd565b$��2q�6q   $��5q�6�   ��6�   �� *EditSegmentCommand+5�      5�      5�      5�      