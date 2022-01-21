using System;
using System.Drawing;

namespace Cpu3DEngine
{
    public partial class Form1
    {
        private void flatShadingRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (flatShadingRadioButton.Checked)
            {
                device.ShadingType = ShadingType.Flat;
            }
        }

        private void gouraudShadingRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (gouraudShadingRadioButton.Checked)
            {
                device.ShadingType = ShadingType.Gouraud;
            }
        }

        private void phongRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (phongRadioButton.Checked)
            {
                device.ShadingType = ShadingType.Phong;
            }
        }

        private void staticCameraRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (staticCameraRadioButton.Checked)
            {
                device.SelectedCamera = staticCamera;
            }
        }

        private void followingCameraRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (followingCameraRadioButton.Checked)
            {
                device.SelectedCamera = followingCamera;
            }
        }

        private void dynamicCameraRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (dynamicCameraRadioButton.Checked)
            {
                device.SelectedCamera = dynamicCamera;
            }
        }

        private void diffuseTrackBar_Scroll(object sender, EventArgs e)
        {
            var value = diffuseTrackBar.Value / 100.0;
            device.Kd = (float)value;
        }

        private void ambientTrackBar_Scroll(object sender, EventArgs e)
        {
            var value = ambientTrackBar.Value / 100.0;
            device.Ka = (float)value;
        }

        private void specularTrackBar_Scroll(object sender, EventArgs e)
        {
            var value = specularTrackBar.Value / 100.0;
            device.Ks = (float)value;
        }

        private void shininessTrackBar_Scroll(object sender, EventArgs e)
        {
            var value = shininessTrackBar.Value;
            device.M = value;
        }

        private void rotationTrackBar_Scroll(object sender, EventArgs e)
        {
            cylinderAngle = (rotationTrackBar.Value - 50) / 50.0;
        }


        private void globalLightCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            globalLight.IsTurnedOn = globalLightCheckBox.Checked;
        }

        private void spotLightCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            spotLight.IsTurnedOn = spotLightCheckBox.Checked;
            if (spotLightCheckBox.Checked)
            {
                cylinder.ChangeMarkedFacesColor(Color.White, true);
            }
            else
            {
                cylinder.ChangeMarkedFacesColor(cylinder.Color, false);
            }
        }

        private void fogCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            device.Fog = fogCheckBox.Checked ? fogGenerator : null;
        }
    }
}
