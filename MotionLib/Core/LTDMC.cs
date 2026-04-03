using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace SqlDemo.MotionLib.Core
{
    public delegate uint DMC3K5K_OPERATE(IntPtr operate_data); 
    public partial class LTDMC
    {
        //���úͶ�ȡ��ӡģʽ����������������/���߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_debug_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_debug_mode(ushort mode, string FileName);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_debug_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_debug_mode(ref ushort mode, IntPtr FileName);
        //---------------------   �忨��ʼ�����ú���  ----------------------
        //��ʼ�����ƿ�����������������/���߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_board_init", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_board_init();
        //Ӳ����λ����������������/���߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_board_reset", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_board_reset();
        //�رտ��ƿ�����������������/���߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_board_close", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_board_close();
        //���ƿ��ȸ�λ��������EtherCAT��RTEX���߿���  
        [DllImport("LTDMC.dll")]
        public static extern short dmc_soft_reset(ushort CardNo);
        //���ƿ��临λ����������������/���߿���
        [DllImport("LTDMC.dll")]
        public static extern short dmc_cool_reset(ushort CardNo);
        //���ƿ���ʼ��λ��������EtherCAT���߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_original_reset", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_original_reset(ushort CardNo);
        //��ȡ���ƿ���Ϣ�б�����������������/���߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_CardInfList", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_CardInfList(ref ushort CardNum, uint[] CardTypeList, ushort[] CardIdList);
        //��ȡ�����汾�ţ�������DMC3000/DMC5X10ϵ�����忨��EtherCAT���߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_card_version", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_card_version(ushort CardNo, ref uint CardVersion);
        //��ȡ���ƿ�Ӳ���Ĺ̼��汾����������������/���߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_card_soft_version", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_card_soft_version(ushort CardNo, ref uint FirmID, ref uint SubFirmID);
        //��ȡ���ƿ���̬��汾����������������/���߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_card_lib_version", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_card_lib_version(ref uint LibVer);
        //��ȡ�����汾�ţ�������DMC3000/DMC5X10ϵ�����忨��EtherCAT���߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_release_version", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_release_version(ushort ConnectNo, byte[] ReleaseVersion);
        //��ȡָ�����������������������忨��
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_total_axes", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_total_axes(ushort CardNo, ref uint TotalAxis);
        //��ȡ����IO��������������������/���߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_total_ionum", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_total_ionum(ushort CardNo, ref ushort TotalIn, ref ushort TotalOut);
        //��ȡ����ADDA������������������������忨��
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_total_adcnum", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_total_adcnum(ushort CardNo, ref ushort TotalIn, ref ushort TotalOut);
        //��ȡָ�����岹����ϵ����������
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_total_liners", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_total_liners(ushort CardNo, ref uint TotalLiner);
        //�����ࣨ������
        [DllImport("LTDMC.dll", EntryPoint = "dmc_board_init_onecard", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_board_init_onecard(ushort CardNo);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_board_close_onecard", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_board_close_onecard(ushort CardNo);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_board_reset_onecard", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_board_reset_onecard(ushort CardNo);

        //���뺯������������������/���߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_write_sn", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_write_sn(ushort CardNo, string new_sn);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_check_sn", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_check_sn(ushort CardNo, string check_sn);
        //����sn20191101��������DMC3000/5X10ϵ�����忨��
        [DllImport("LTDMC.dll")]
        public static extern short dmc_enter_password_ex(ushort CardNo, string str_pass);

        //---------------------�˶�ģ������ģʽ------------------
        //����ģʽ���������������忨��	
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_pulse_outmode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_pulse_outmode(ushort CardNo, ushort axis, ushort outmode);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_pulse_outmode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_pulse_outmode(ushort CardNo, ushort axis, ref ushort outmode);
        //���嵱����������EtherCAT���߿���RTEX���߿���DMC5000/5X10ϵ�����忨��
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_equiv", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_equiv(ushort CardNo, ushort axis, ref double equiv);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_equiv", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_equiv(ushort CardNo, ushort axis, double equiv);
        //�����϶(����)��������DMC5000ϵ�����忨��	
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_backlash_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_backlash_unit(ushort CardNo, ushort axis, double backlash); 
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_backlash_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_backlash_unit(ushort CardNo, ushort axis, ref double backlash);

        //ͨ���ļ�����
        [DllImport("LTDMC.dll", EntryPoint = "dmc_download_file", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_download_file(ushort CardNo, string pfilename, byte[] pfilenameinControl, ushort filetype);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_upload_file", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_upload_file(ushort CardNo, string pfilename, byte[] pfilenameinControl, ushort filetype);
        //�����ڴ��ļ� ���߿���������EtherCAT���߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_download_memfile", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_download_memfile(ushort CardNo, byte[] pbuffer, uint buffsize, byte[] pfilenameinControl, ushort filetype);
        //�ϴ��ڴ��ļ���������EtherCAT���߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_upload_memfile", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_upload_memfile(ushort CardNo, byte[] pbuffer, uint buffsize, byte[] pfilenameinControl, ref uint puifilesize, ushort filetype);
        //�ļ����ȣ���������������/���߿���
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_progress(ushort CardNo, ref float process);
        //���ز����ļ�����������������/���߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_download_configfile", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_download_configfile(ushort CardNo, string FileName);
        //���ع̼��ļ�����������������/���߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_download_firmware", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_download_firmware(ushort CardNo, string FileName);

        //----------------------��λ�쳣����-------------------------------	
        //���ö�ȡ����λ������������E3032���߿���R3032���߿���DMC3000/5000/5X10ϵ�����忨��	
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_softlimit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_softlimit(ushort CardNo, ushort axis, ushort enable, ushort source_sel, ushort SL_action, int N_limit, int P_limit);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_softlimit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_softlimit(ushort CardNo, ushort axis, ref ushort enable, ref ushort source_sel, ref ushort SL_action, ref int N_limit, ref int P_limit);
        //���ö�ȡ����λ����unit��������DMC5X10ϵ�����忨��E5032���߿���
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_softlimit_unit(ushort CardNo, ushort axis, ushort enable, ushort source_sel, ushort SL_action, double N_limit, double P_limit);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_softlimit_unit(ushort CardNo, ushort axis, ref ushort enable, ref ushort source_sel, ref ushort SL_action, ref double N_limit, ref double P_limit);
        //���ö�ȡEL�źţ��������������忨��
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_el_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_el_mode(ushort CardNo, ushort axis, ushort el_enable, ushort el_logic, ushort el_mode);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_el_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_el_mode(ushort CardNo, ushort axis, ref ushort el_enable, ref ushort el_logic, ref ushort el_mode);
        //���ö�ȡEMG�źţ���������������/���߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_emg_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_emg_mode(ushort CardNo, ushort axis, ushort enable, ushort emg_logic);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_emg_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_emg_mode(ushort CardNo, ushort axis, ref ushort enbale, ref ushort emg_logic);
        //�ⲿ����ֹͣ�źż�����ֹͣʱ�����ã�����Ϊ��λ��������
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_dstp_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_dstp_mode(ushort CardNo, ushort axis, ushort enable, ushort logic, uint time);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_dstp_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_dstp_mode(ushort CardNo, ushort axis, ref ushort enable, ref ushort logic, ref uint time);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_dstp_time", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_dstp_time(ushort CardNo, ushort axis, uint time);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_dstp_time", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_dstp_time(ushort CardNo, ushort axis, ref uint time);
        //�ⲿ����ֹͣ�źż�����ֹͣʱ�����ã���Ϊ��λ���������������忨��
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_io_dstp_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_io_dstp_mode(ushort CardNo, ushort axis, ushort enable, ushort logic);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_io_dstp_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_io_dstp_mode(ushort CardNo, ushort axis, ref ushort enable, ref ushort logic);
        //��λ�˶�����ֹͣʱ�����ö�ȡ���������������忨�����߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_dec_stop_time", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_dec_stop_time(ushort CardNo, ushort axis, double stop_time);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_dec_stop_time", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_dec_stop_time(ushort CardNo, ushort axis, ref double stop_time);
        //�岹����ֹͣ�źźͼ���ʱ�����ã�������DMC5X10ϵ�����忨��EthreCAT���߿���
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_vector_dec_stop_time(ushort CardNo, ushort Crd, double stop_time);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_vector_dec_stop_time(ushort CardNo, ushort Crd, ref double stop_time);
        //IO����ֹͣ���루������DMC3000��DMC5X10ϵ�����忨��
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_dec_stop_dist(ushort CardNo, ushort axis, int dist);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_dec_stop_dist(ushort CardNo, ushort axis, ref int dist);
        //IO����ֹͣ��֧��pmove/vmove�˶���������DMC3000��DMC5X10ϵ�����忨��
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_io_exactstop(ushort CardNo, ushort axis, ushort ioNum, ushort[] ioList, ushort enable, ushort valid_logic, ushort action, ushort move_dir);       
        //����ͨ������ڵ�һλ����ֹͣIO�ڣ�������
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_io_dstp_bitno(ushort CardNo, ushort axis, ushort bitno, double filter);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_io_dstp_bitno(ushort CardNo, ushort axis, ref ushort bitno, ref double filter);

        //---------------------------�����˶�----------------------
        //�趨��ȡ�ٶ����߲���	���������������忨��
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_profile", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_profile(ushort CardNo, ushort axis, double Min_Vel, double Max_Vel, double Tacc, double Tdec, double stop_vel);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_profile", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_profile(ushort CardNo, ushort axis, ref double Min_Vel, ref double Max_Vel, ref double Tacc, ref double Tdec, ref double stop_vel);
        //�ٶ�����(���嵱��)��������EtherCAT���߿���RTEX���߿���DMC5000/5X10ϵ�����忨��	
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_profile_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_profile_unit(ushort CardNo, ushort Axis, double Min_Vel, double Max_Vel, double Tacc, double Tdec, double Stop_Vel);   //�����ٶȲ���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_profile_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_profile_unit(ushort CardNo, ushort Axis, ref double Min_Vel, ref double Max_Vel, ref double Tacc, ref double Tdec, ref double Stop_Vel);
        //�ٶ��������ã����ٶ�ֵ��ʾ(����)���������������忨��
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_acc_profile", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_acc_profile(ushort CardNo, ushort axis, double Min_Vel, double Max_Vel, double Tacc, double Tdec, double stop_vel);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_acc_profile", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_acc_profile(ushort CardNo, ushort axis, ref double Min_Vel, ref double Max_Vel, ref double Tacc, ref double Tdec, ref double stop_vel);
        //�ٶ��������ã����ٶ�ֵ��ʾ(����)��������EtherCAT���߿���RTEX���߿���DMC5000/5X10ϵ�����忨��
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_profile_unit_acc", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_profile_unit_acc(ushort CardNo, ushort Axis, double Min_Vel, double Max_Vel, double Tacc, double Tdec, double Stop_Vel);   //�����ٶȲ���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_profile_unit_acc", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_profile_unit_acc(ushort CardNo, ushort Axis, ref double Min_Vel, ref double Max_Vel, ref double Tacc, ref double Tdec, ref double Stop_Vel);      
        //���ö�ȡƽ���ٶ����߲�������������������/���߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_s_profile", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_s_profile(ushort CardNo, ushort axis, ushort s_mode, double s_para);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_s_profile", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_s_profile(ushort CardNo, ushort axis, ushort s_mode, ref double s_para);            
        //��λ�˶�(����)���������������忨��
        [DllImport("LTDMC.dll", EntryPoint = "dmc_pmove", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_pmove(ushort CardNo, ushort axis, int Dist, ushort posi_mode);
        //��λ�˶�(����)��������EtherCAT���߿���RTEX���߿���DMC5000/5X10ϵ�����忨��
        [DllImport("LTDMC.dll", EntryPoint = "dmc_pmove_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_pmove_unit(ushort CardNo, ushort axis, double Dist, ushort posi_mode);  
        //ָ����������λ���˶� ͬʱ�����ٶȺ�Sʱ��(����)��������DMC5X10ϵ�����忨��	
        [DllImport("LTDMC.dll")]
        public static extern short dmc_pmove_extern(ushort CardNo, ushort axis, double dist, double Min_Vel, double Max_Vel, double Tacc, double Tdec, double stop_Vel, double s_para, ushort posi_mode);
        //���߱�λ(����)���˶��иı�Ŀ��λ�ã��������������忨��
        [DllImport("LTDMC.dll", EntryPoint = "dmc_reset_target_position", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_reset_target_position(ushort CardNo, ushort axis, int dist, ushort posi_mode);
        //���ٱ�λ(����)��������EtherCAT���߿���RTEX���߿���DMC5000/5X10ϵ�����忨��
        [DllImport("LTDMC.dll", EntryPoint = "dmc_reset_target_position_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_reset_target_position_unit(ushort CardNo, ushort Axis, double New_Pos); 
        //���߱���(����)���˶��иı�ָ����ĵ�ǰ�˶��ٶȣ��������������忨��
        [DllImport("LTDMC.dll", EntryPoint = "dmc_change_speed", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_change_speed(ushort CardNo, ushort axis, double Curr_Vel, double Taccdec);
        //���߱���(����)���˶��иı�ָ����ĵ�ǰ�˶��ٶȣ�������EtherCAT���߿���RTEX���߿���DMC5000/5X10ϵ�����忨��
        [DllImport("LTDMC.dll", EntryPoint = "dmc_change_speed_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_change_speed_unit(ushort CardNo, ushort Axis, double New_Vel, double Taccdec);    
        //�����˶����ǿ�иı�Ŀ��λ�ã��������������忨��
        [DllImport("LTDMC.dll", EntryPoint = "dmc_update_target_position", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_update_target_position(ushort CardNo, ushort axis, int dist, ushort posi_mode);
        //ǿ�б�λ��չ��������DMC5X10ϵ�����忨��
        [DllImport("LTDMC.dll")]
        public static extern short dmc_update_target_position_extern(ushort CardNo, ushort axis, double mid_pos, double aim_pos, double vel, ushort posi_mode);
        //���߱���(����)���˶��иı�ָ����ĵ�ǰ�˶��ٶȣ�������EtherCAT���߿���RTEX���߿���DMC5000/5X10ϵ�����忨��
        [DllImport("LTDMC.dll", EntryPoint = "dmc_update_target_position_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_update_target_position_unit(ushort CardNo, ushort Axis, double New_Pos);           
        //---------------------JOG�˶�--------------------
        //���������ٶ��˶�����������������/���߿���	
        [DllImport("LTDMC.dll", EntryPoint = "dmc_vmove", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_vmove(ushort CardNo, ushort axis, ushort dir);

        //---------------------�岹�˶�--------------------
        //�岹�ٶ�����(����)��������DMC3000ϵ�����忨��
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_vector_profile_multicoor", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_vector_profile_multicoor(ushort CardNo, ushort Crd, double Min_Vel, double Max_Vel, double Tacc, double Tdec, double Stop_Vel);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_vector_profile_multicoor", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_vector_profile_multicoor(ushort CardNo, ushort Crd, ref double Min_Vel, ref double Max_Vel, ref double Taccdec, ref double Tdec, ref double Stop_Vel);       
        //���ö�ȡƽ���ٶ����߲�����������DMC3000ϵ�����忨��
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_vector_s_profile_multicoor(ushort CardNo, ushort Crd, ushort s_mode, double s_para);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_vector_s_profile_multicoor(ushort CardNo, ushort Crd, ushort s_mode, ref double s_para);
        //�岹�ٶȲ���(����)��������EtherCAT���߿���RTEX���߿���DMC5000/5X10ϵ�����忨��
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_vector_profile_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_vector_profile_unit(ushort CardNo, ushort Crd, double Min_Vel, double Max_Vel, double Tacc, double Tdec, double Stop_Vel);   //���β岹�ٶȲ���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_vector_profile_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_vector_profile_unit(ushort CardNo, ushort Crd, ref double Min_Vel, ref double Max_Vel, ref double Tacc, ref double Tdec, ref double Stop_Vel);
        //����ƽ���ٶ����߲�����������EtherCAT���߿���RTEX���߿���DMC5000/5X10ϵ�����忨��
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_vector_s_profile", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_vector_s_profile(ushort CardNo, ushort Crd, ushort s_mode, double s_para);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_vector_s_profile", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_vector_s_profile(ushort CardNo, ushort Crd, ushort s_mode, ref double s_para);
        //ֱ�߲岹�˶���������DMC3000ϵ�����忨��	
        [DllImport("LTDMC.dll", EntryPoint = "dmc_line_multicoor", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_line_multicoor(ushort CardNo, ushort crd, ushort axisNum, ushort[] axisList, int[] DistList, ushort posi_mode);
        //Բ���岹�˶���������DMC3000ϵ�����忨��
        [DllImport("LTDMC.dll", EntryPoint = "dmc_arc_move_multicoor", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_arc_move_multicoor(ushort CardNo, ushort crd, ushort[] AxisList, int[] Target_Pos, int[] Cen_Pos, ushort Arc_Dir, ushort posi_mode);
        //ֱ�߲岹(����)��������EtherCAT���߿���RTEX���߿���DMC5000/5X10ϵ�����忨��
        [DllImport("LTDMC.dll", EntryPoint = "dmc_line_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_line_unit(ushort CardNo, ushort Crd, ushort AxisNum, ushort[] AxisList, double[] Target_Pos, ushort posi_mode);    //����ֱ��
        //Բ��Բ���岹(����)��������EtherCAT���߿���RTEX���߿���DMC5000/5X10ϵ�����忨��
        [DllImport("LTDMC.dll", EntryPoint = "dmc_arc_move_center_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_arc_move_center_unit(ushort CardNo, ushort Crd, ushort AxisNum, ushort[] AxisList, double[] Target_Pos, double[] Cen_Pos, ushort Arc_Dir, int Circle, ushort posi_mode);     //Բ���յ�ʽԲ��/������/������
        //�뾶Բ���岹(����)��������EtherCAT���߿���RTEX���߿���DMC5000/5X10ϵ�����忨��
        [DllImport("LTDMC.dll", EntryPoint = "dmc_arc_move_radius_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_arc_move_radius_unit(ushort CardNo, ushort Crd, ushort AxisNum, ushort[] AxisList, double[] Target_Pos, double Arc_Radius, ushort Arc_Dir, int Circle, ushort posi_mode);    //�뾶�յ�ʽԲ��/������
        //����Բ���岹(����)��������EtherCAT���߿���RTEX���߿���DMC5000/5X10ϵ�����忨��
        [DllImport("LTDMC.dll", EntryPoint = "dmc_arc_move_3points_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_arc_move_3points_unit(ushort CardNo, ushort Crd, ushort AxisNum, ushort[] AxisList, double[] Target_Pos, double[] Mid_Pos, int Circle, ushort posi_mode);     //����ʽԲ��/������
        //���β岹(����)��������EtherCAT���߿���RTEX���߿���DMC5000/5X10ϵ�����忨��
        [DllImport("LTDMC.dll", EntryPoint = "dmc_rectangle_move_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_rectangle_move_unit(ushort CardNo, ushort Crd, ushort AxisNum, ushort[] AxisList, double[] TargetPos, double[] MaskPos, int Count, ushort rect_mode, ushort posi_mode);     //��������岹�����β岹ָ��

        //----------------------PVT�˶�---------------------------
        //PVT�˶��ɰ� ���������������忨��
        [DllImport("LTDMC.dll", EntryPoint = "dmc_PvtTable", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_PvtTable(ushort CardNo, ushort iaxis, uint count, double[] pTime, int[] pPos, double[] pVel);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_PtsTable", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_PtsTable(ushort CardNo, ushort iaxis, uint count, double[] pTime, int[] pPos, double[] pPercent);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_PvtsTable", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_PvtsTable(ushort CardNo, ushort iaxis, uint count, double[] pTime, int[] pPos, double velBegin, double velEnd);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_PttTable", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_PttTable(ushort CardNo, ushort iaxis, uint count, double[] pTime, int[] pPos);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_PvtMove", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_PvtMove(ushort CardNo, ushort AxisNum, ushort[] AxisList);
        //PVT����������
        [DllImport("LTDMC.dll")]
        public static extern short dmc_PttTable_add(ushort CardNo, ushort iaxis, ushort count, double[] pTime, long[] pPos);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_PtsTable_add(ushort CardNo, ushort iaxis, ushort count, double[] pTime, long[] pPos, double[] pPercent);
        //��ȡpvtʣ��ռ�
        [DllImport("LTDMC.dll")]
        public static extern short dmc_pvt_get_remain_space(ushort CardNo, ushort iaxis);
        //PVT�˶� ���߿��¹滮��������EtherCAT���߿�
        [DllImport("LTDMC.dll")]
        public static extern short dmc_pvt_table_unit(ushort CardNo, ushort iaxis, uint count, double[] pTime, double[] pPos, double[] pVel);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_pts_table_unit(ushort CardNo, ushort iaxis, uint count, double[] pTime, double[] pPos, double[] pPercent);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_pvts_table_unit(ushort CardNo, ushort iaxis, uint count, double[] pTime, double[] pPos, double velBegin, double velEnd);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_ptt_table_unit(ushort CardNo, ushort iaxis, uint count, double[] pTime, double[] pPos);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_pvt_move(ushort CardNo, ushort AxisNum, ushort[] AxisList);
        //�����ࣨ������
        [DllImport("LTDMC.dll")]
        public static extern short dmc_SetGearProfile(ushort CardNo, ushort axis, ushort MasterType, ushort MasterIndex, int MasterEven, int SlaveEven, uint MasterSlope);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_GetGearProfile(ushort CardNo, ushort axis, ref ushort MasterType, ref ushort MasterIndex, ref uint MasterEven, ref uint SlaveEven, ref uint MasterSlope);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_GearMove(ushort CardNo, ushort AxisNum, ushort[] AxisList);
              
        //--------------------�����˶�---------------------
        //���ö�ȡHOME�źţ��������������忨��
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_home_pin_logic", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_home_pin_logic(ushort CardNo, ushort axis, ushort org_logic, double filter);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_home_pin_logic", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_home_pin_logic(ushort CardNo, ushort axis, ref ushort org_logic, ref double filter);
        //�趨��ȡָ����Ļ�ԭ��ģʽ���������������忨��
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_homemode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_homemode(ushort CardNo, ushort axis, ushort home_dir, double vel, ushort mode, ushort EZ_count);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_homemode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_homemode(ushort CardNo, ushort axis, ref ushort home_dir, ref double vel, ref ushort home_mode, ref ushort EZ_count);
        //���û�������λ�Ƿ��ң�������DMC3000/5X10ϵ�����忨��
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_home_el_return(ushort CardNo, ushort axis, ushort enable);
        //��ȡ��������λ����ʹ�ܣ�������DMC3000/5X10ϵ�����忨��
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_home_el_return(ushort CardNo, ushort axis, ref ushort enable);
        //�������㣨�������������忨��
        [DllImport("LTDMC.dll", EntryPoint = "dmc_home_move", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_home_move(ushort CardNo, ushort axis);
        //���ö�ȡ�����ٶȲ�����������Rtex���߿���
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_home_profile_unit(ushort CardNo, ushort axis, double Low_Vel, double High_Vel, double Tacc, double Tdec);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_home_profile_unit(ushort CardNo, ushort axis, ref double Low_Vel, ref double High_Vel, ref double Tacc, ref double Tdec);
        //��ȡ����ִ��״̬����������������/���߿���
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_home_result(ushort CardNo, ushort axis, ref ushort state);
        //���ö�ȡ����ƫ����������ģʽ��������DMC5X10���忨��
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_home_position_unit(ushort CardNo, ushort axis, ushort enable, double position);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_home_position_unit(ushort CardNo, ushort axis, ref ushort enable, ref double position);
        //��������
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_el_home(ushort CardNo, ushort axis, ushort mode);
        //����ƫ��ģʽ������������
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_home_shift_param(ushort CardNo, ushort axis, ushort pos_clear_mode, double ShiftValue);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_home_shift_param(ushort CardNo, ushort axis, ref ushort pos_clear_mode, ref double ShiftValue);
        //���û���ƫ������ƫ��ģʽ��������DMC3000ϵ�����忨��
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_home_position(ushort CardNo, ushort axis, ushort enable, double position);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_home_position(ushort CardNo, ushort axis, ref ushort enable, ref double position);
        //���û�����λ���루������
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_home_soft_limit(ushort CardNo, ushort Axis, int N_limit, int P_limit);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_home_soft_limit(ushort CardNo, ushort Axis, ref int N_limit, ref int P_limit);
       
        //--------------------ԭ������-------------------
        //���ö�ȡEZ����ģʽ���������������忨��
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_homelatch_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_homelatch_mode(ushort CardNo, ushort axis, ushort enable, ushort logic, ushort source);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_homelatch_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_homelatch_mode(ushort CardNo, ushort axis, ref ushort enable, ref ushort logic, ref ushort source);
        //��ȡԭ�������־���������������忨��
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_homelatch_flag", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_homelatch_flag(ushort CardNo, ushort axis);
        //���ԭ�������־���������������忨��
        [DllImport("LTDMC.dll", EntryPoint = "dmc_reset_homelatch_flag", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_reset_homelatch_flag(ushort CardNo, ushort axis);
        //��ȡԭ������ֵ���������������忨��
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_homelatch_value", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int dmc_get_homelatch_value(ushort CardNo, ushort axis);
        //��ȡԭ������ֵ��unit����������DMC5X10ϵ�����忨��
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_homelatch_value_unit(ushort CardNo, ushort axis, ref double pos);

        //--------------------EZ����-------------------
        //���ö�ȡEZ����ģʽ���������������忨��
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_ezlatch_mode(ushort CardNo, ushort axis, ushort enable, ushort logic, ushort source);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_ezlatch_mode(ushort CardNo, ushort axis, ref ushort enable, ref ushort logic, ref ushort source);
        //��ȡEZ�����־���������������忨��
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_ezlatch_flag(ushort CardNo, ushort axis);
        //���EZ�����־���������������忨��
        [DllImport("LTDMC.dll")]
        public static extern short dmc_reset_ezlatch_flag(ushort CardNo, ushort axis);
        //��ȡEZ����ֵ���������������忨��
        [DllImport("LTDMC.dll")]
        public static extern int dmc_get_ezlatch_value(ushort CardNo, ushort axis);
        //��ȡEZ����ֵ��unit����������DMC5X10ϵ�����忨��
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_ezlatch_value_unit(ushort CardNo, ushort axis, ref double pos);

        //--------------------�����˶�---------------------	
        //���ö�ȡ����ͨ�����������������忨��
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_handwheel_channel", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_handwheel_channel(ushort CardNo, ushort index);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_handwheel_channel", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_handwheel_channel(ushort CardNo, ref ushort index);
        //���ö�ȡ�������������źŵĹ�����ʽ����������������/���߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_handwheel_inmode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_handwheel_inmode(ushort CardNo, ushort axis, ushort inmode, int multi, double vh);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_handwheel_inmode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_handwheel_inmode(ushort CardNo, ushort axis, ref ushort inmode, ref int multi, ref double vh);
        //���ö�ȡ�������������źŵĹ�����ʽ�������ͱ��ʣ�������DMC5X10ϵ�����忨��
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_handwheel_inmode_decimals", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_handwheel_inmode_decimals(ushort CardNo, ushort axis, ushort inmode, double multi, double vh);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_handwheel_inmode_decimals", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_handwheel_inmode_decimals(ushort CardNo, ushort axis, ref ushort inmode, ref double multi, ref double vh);
        //���ö�ȡ�������������źŵĹ�����ʽ����������������/���߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_handwheel_inmode_extern", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_handwheel_inmode_extern(ushort CardNo, ushort inmode, ushort AxisNum, ushort[] AxisList, int[] multi);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_handwheel_inmode_extern", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_handwheel_inmode_extern(ushort CardNo, ref ushort inmode, ref ushort AxisNum, ushort[] AxisList, int[] multi);
        //���ö�ȡ�������������źŵĹ�����ʽ�������ͱ��ʣ�������DMC5X10ϵ�����忨��
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_handwheel_inmode_extern_decimals(ushort CardNo, ushort inmode, ushort AxisNum, ushort[] AxisList, double[] multi);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_handwheel_inmode_extern_decimals(ushort CardNo, ref ushort inmode, ref ushort AxisNum, ushort[] AxisList, double[] multi);
        //���������˶�����������������/���߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_handwheel_move", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_handwheel_move(ushort CardNo, ushort axis);    
        //�����˶� �������ߵ�����ģʽ  (����)
        [DllImport("LTDMC.dll")]
        public static extern short dmc_handwheel_set_axislist(ushort CardNo, ushort AxisSelIndex, ushort AxisNum, ushort[] AxisList);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_handwheel_get_axislist(ushort CardNo, ushort AxisSelIndex, ref ushort AxisNum, ushort[] AxisList);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_handwheel_set_ratiolist(ushort CardNo, ushort AxisSelIndex, ushort StartRatioIndex, ushort RatioSelNum, double[] RatioList);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_handwheel_get_ratiolist(ushort CardNo, ushort AxisSelIndex, ushort StartRatioIndex, ushort RatioSelNum, double[] RatioList);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_handwheel_set_mode(ushort CardNo, ushort InMode, ushort IfHardEnable);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_handwheel_get_mode(ushort CardNo, ref ushort InMode, ref ushort IfHardEnable);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_handwheel_set_index(ushort CardNo, ushort AxisSelIndex, ushort RatioSelIndex);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_handwheel_get_index(ushort CardNo, ref ushort AxisSelIndex, ref ushort RatioSelIndex);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_handwheel_stop(ushort CardNo);
        
        //-------------------------��������-------------------
        //���ö�ȡָ�����LTC�źţ��������������忨��
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_ltc_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_ltc_mode(ushort CardNo, ushort axis, ushort ltc_logic, ushort ltc_mode, double filter);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_ltc_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_ltc_mode(ushort CardNo, ushort axis, ref ushort ltc_logic, ref ushort ltc_mode, ref double filter);
        //���ö������淽ʽ���������������忨��
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_latch_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_latch_mode(ushort CardNo, ushort axis, ushort all_enable, ushort latch_source, ushort triger_chunnel);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_latch_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_latch_mode(ushort CardNo, ushort axis, ref ushort all_enable, ref ushort latch_source, ref ushort triger_chunnel);
        //��ȡ��������������ֵ���������������忨��
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_latch_value", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int dmc_get_latch_value(ushort CardNo, ushort axis);
        //��ȡ��������������ֵunit��������DMC5X10ϵ�����忨��
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_latch_value_unit(ushort CardNo, ushort axis, ref double pos_by_mm);
        //��ȡ��������־���������������忨��
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_latch_flag", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_latch_flag(ushort CardNo, ushort axis);
        //��λ��������־���������������忨��
        [DllImport("LTDMC.dll", EntryPoint = "dmc_reset_latch_flag", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_reset_latch_flag(ushort CardNo, ushort axis);
        //������ȡֵ������DMC3000ϵ�����忨��
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_latch_value_extern", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int dmc_get_latch_value_extern(ushort CardNo, ushort axis, ushort Index);
        //�������棨Ԥ����
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_latch_value_extern_unit(ushort CardNo, ushort axis, ushort index, ref double pos_by_mm);//������ȡֵ��ȡ 
        //��ȡ�������������DMC3000ϵ�����忨��
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_latch_flag_extern", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_latch_flag_extern(ushort CardNo, ushort axis);
        //���ö�ȡLTC����������������������忨��
        [DllImport("LTDMC.dll", EntryPoint = "dmc_SetLtcOutMode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_SetLtcOutMode(ushort CardNo, ushort axis, ushort enable, ushort bitno);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_GetLtcOutMode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_GetLtcOutMode(ushort CardNo, ushort axis, ref ushort enable, ref ushort bitno);
        //LTC�˿ڴ�����ʱ��ͣʱ�� ��λus���������������忨��
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_latch_stop_time", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_latch_stop_time(ushort CardNo, ushort axis, int time);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_latch_stop_time", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_latch_stop_time(ushort CardNo, ushort axis, ref int time);
        //����/�ض�LTC�˿ڴ�����ʱ��ͣ�����ã�������EtherCAT����ϵ�п���
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_latch_stop_axis(ushort CardNo, ushort latch, ushort num, ushort[] axislist);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_latch_stop_axis(ushort CardNo, ushort latch, ref ushort num, ushort[] axislist);

        //----------------------�������� ���߿�---------------------------
        //����������������ģʽ0-�������棬1-�������棻�������0-�½��أ�1-�����أ�2-˫���أ��˲�ʱ�䣬��λus���������������߿���
        [DllImport("LTDMC.dll")]
        public static extern short dmc_ltc_set_mode(ushort CardNo, ushort latch, ushort ltc_mode, ushort ltc_logic, double filter);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_ltc_get_mode(ushort CardNo, ushort latch, ref ushort ltc_mode, ref ushort ltc_logic, ref double filter);
        //��������Դ��0-ָ��λ�ã�1-����������λ�ã��������������߿���
        [DllImport("LTDMC.dll")]
        public static extern short dmc_ltc_set_source(ushort CardNo, ushort latch, ushort axis, ushort ltc_source);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_ltc_get_source(ushort CardNo, ushort latch, ushort axis, ref ushort ltc_source);
        //��λ���������������������߿���
        [DllImport("LTDMC.dll")]
        public static extern short dmc_ltc_reset(ushort CardNo, ushort latch);
        //��ȡ����������������������߿���
        [DllImport("LTDMC.dll")]
        public static extern short dmc_ltc_get_number(ushort CardNo, ushort latch, ushort axis, ref int number);
        //��ȡ����ֵ���������������߿���
        [DllImport("LTDMC.dll")]
        public static extern short dmc_ltc_get_value_unit(ushort CardNo, ushort latch, ushort axis, ref double value);

        //-----------------------������ ���п�---------------------------------
        //����������������ģʽ0-�������棬1-�������棻�������0-�½��أ�1-�����أ�2-˫���أ��˲�ʱ�䣬��λus��������DMC5X10/3000ϵ�����忨�����߿���
        [DllImport("LTDMC.dll")]
        public static extern short dmc_softltc_set_mode(ushort ConnectNo, ushort latch, ushort ltc_enable, ushort ltc_mode, ushort ltc_inbit, ushort ltc_logic, double filter);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_softltc_get_mode(ushort ConnectNo, ushort latch, ref ushort ltc_enable, ref ushort ltc_mode, ref ushort ltc_inbit, ref ushort ltc_logic, ref double filter);
        //��������Դ��0-ָ��λ�ã�1-����������λ�ã�������DMC5X10/3000ϵ�����忨�����߿���
        [DllImport("LTDMC.dll")]
        public static extern short dmc_softltc_set_source(ushort ConnectNo, ushort latch, ushort axis, ushort ltc_source);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_softltc_get_source(ushort ConnectNo, ushort latch, ushort axis, ref ushort ltc_source);
        //��λ��������������DMC5X10/3000ϵ�����忨�����߿���
        [DllImport("LTDMC.dll")]
        public static extern short dmc_softltc_reset(ushort ConnectNo, ushort latch);
        //��ȡ���������������DMC5X10/3000ϵ�����忨�����߿���
        [DllImport("LTDMC.dll")]
        public static extern short dmc_softltc_get_number(ushort ConnectNo, ushort latch, ushort axis, ref int number);
        //��ȡ����ֵ��������DMC5X10ϵ�����忨���������߿���
        [DllImport("LTDMC.dll")]
        public static extern short dmc_softltc_get_value_unit(ushort ConnectNo, ushort latch, ushort axis, ref double value);

        //----------------------�������λ�ñȽ�-----------------------	
        //���ö�ȡ�Ƚ�������������������/���߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_compare_set_config", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_compare_set_config(ushort CardNo, ushort axis, ushort enable, ushort cmp_source);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_compare_get_config", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_compare_get_config(ushort CardNo, ushort axis, ref ushort enable, ref ushort cmp_source);
        //������бȽϵ㣨��������������/���߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_compare_clear_points", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_compare_clear_points(ushort CardNo, ushort axis);
        //���ӱȽϵ㣨�������������忨��
        [DllImport("LTDMC.dll", EntryPoint = "dmc_compare_add_point", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_compare_add_point(ushort CardNo, ushort axis, int pos, ushort dir, ushort action, uint actpara);
        //���ӱȽϵ㣨����������DMC5X10���忨��
        [DllImport("LTDMC.dll")]
        public static extern short dmc_compare_add_point_unit(ushort CardNo, ushort cmp, double pos, ushort dir, ushort action, uint actpara);        
        //���ӱȽϵ㣨������E3032/R3032��
        [DllImport("LTDMC.dll")]
        public static extern short dmc_compare_add_point_cycle(ushort CardNo, ushort cmp, int pos, ushort dir, uint bitno, uint cycle, ushort level);
        //���ӱȽϵ�unit��������E5032��
        [DllImport("LTDMC.dll")]
        public static extern short dmc_compare_add_point_cycle_unit(ushort CardNo, ushort cmp, double pos, ushort dir, uint bitno, uint cycle, ushort level);
        //��ȡ��ǰ�Ƚϵ㣨�������������忨��Rtex���߿���E3032����
        [DllImport("LTDMC.dll", EntryPoint = "dmc_compare_get_current_point", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_compare_get_current_point(ushort CardNo, ushort axis, ref int pos);
        //��ȡ��ǰ�Ƚϵ㣨������DMC5X10ϵ�����忨��E5032���߿���
        [DllImport("LTDMC.dll")]
        public static extern short dmc_compare_get_current_point_unit(ushort CardNo, ushort cmp, ref double pos);
        //��ѯ�Ѿ��ȽϹ��ĵ㣨��������������/���߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_compare_get_points_runned", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_compare_get_points_runned(ushort CardNo, ushort axis, ref int pointNum);
        //��ѯ���Լ���ıȽϵ���������������������/���߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_compare_get_points_remained", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_compare_get_points_remained(ushort CardNo, ushort axis, ref int pointNum);
        
        //-------------------��ά����λ�ñȽ�-----------------------
        //���ö�ȡ�Ƚ������������������忨��EtherCAT���߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_compare_set_config_extern", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_compare_set_config_extern(ushort CardNo, ushort enable, ushort cmp_source);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_compare_get_config_extern", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_compare_get_config_extern(ushort CardNo, ref ushort enable, ref ushort cmp_source);
        //������бȽϵ㣨�������������忨��EtherCAT���߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_compare_clear_points_extern", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_compare_clear_points_extern(ushort CardNo);
        //��������λ�ñȽϵ㣨�������������忨��
        [DllImport("LTDMC.dll", EntryPoint = "dmc_compare_add_point_extern", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_compare_add_point_extern(ushort CardNo, ushort[] axis, int[] pos, ushort[] dir, ushort action, uint actpara);
        //��ȡ��ǰ�Ƚϵ㣨�������������忨��EtherCAT���߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_compare_get_current_point_extern", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_compare_get_current_point_extern(ushort CardNo, int[] pos);
        //��ȡ��ǰ�Ƚϵ�unit��������DMC5000/5X10ϵ�����忨��E5032���߿���
        [DllImport("LTDMC.dll")]
        public static extern short dmc_compare_get_current_point_extern_unit(ushort CardNo, double[] pos);
        //��������λ�ñȽϵ㣨������DMC5X10���忨��      
        [DllImport("LTDMC.dll")]
        public static extern short dmc_compare_add_point_extern_unit(ushort CardNo, ushort[] axis, double[] pos, ushort[] dir, ushort action, uint actpara);
        //���Ӷ�ά����λ�ñȽϵ㣨������EtherCAT����ϵ�п���
        [DllImport("LTDMC.dll")]
        public static extern short dmc_compare_add_point_cycle_2d(ushort CardNo, ushort[] axis, double[] pos, ushort[] dir, uint bitno, uint cycle, ushort level);
        //��ѯ�Ѿ��ȽϹ��ĵ㣨�������������忨��EtherCAT���߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_compare_get_points_runned_extern", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_compare_get_points_runned_extern(ushort CardNo, ref int pointNum);
        //��ѯ���Լ���ıȽϵ��������������������忨��EtherCAT���߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_compare_get_points_remained_extern", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_compare_get_points_remained_extern(ushort CardNo, ref int pointNum);
        //����λ�ñȽϣ�������
        [DllImport("LTDMC.dll")]
        public static extern short dmc_compare_set_config_multi(ushort CardNo, ushort queue, ushort enable, ushort axis, ushort cmp_source);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_compare_get_config_multi(ushort CardNo, ushort queue, ref ushort enable, ref ushort axis, ref ushort cmp_source);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_compare_add_point_multi(ushort CardNo, ushort cmp, int pos, ushort dir, ushort action, uint actpara, double times);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_compare_add_point_multi_unit(ushort CardNo, ushort cmp, double pos, ushort dir, ushort action, uint actpara, double times);//���ӱȽϵ� ��ǿ
        
        //----------- �������λ�ñȽ�-----------------------        
        //���ö�ȡ���ٱȽ�ģʽ���������������忨�����߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_hcmp_set_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_hcmp_set_mode(ushort CardNo, ushort hcmp, ushort cmp_enable);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_hcmp_get_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_hcmp_get_mode(ushort CardNo, ushort hcmp, ref ushort cmp_enable);
        //���ø��ٱȽϲ������������������忨�����߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_hcmp_set_config", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_hcmp_set_config(ushort CardNo, ushort hcmp, ushort axis, ushort cmp_source, ushort cmp_logic, int time);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_hcmp_get_config", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_hcmp_get_config(ushort CardNo, ushort hcmp, ref ushort axis, ref ushort cmp_source, ref ushort cmp_logic, ref int time);
        //���ٱȽ�ģʽ��չ��������
        [DllImport("LTDMC.dll")]
        public static extern short dmc_hcmp_set_config_extern(ushort CardNo, ushort hcmp, ushort axis, ushort cmp_source, ushort cmp_logic, ushort cmp_mode, int dist, int time);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_hcmp_get_config_extern(ushort CardNo, ushort hcmp, ref ushort axis, ref ushort cmp_source, ref ushort cmp_logic, ref ushort cmp_mode, ref int dist, ref int time);
        //���ӱȽϵ㣨�������������忨��E3032���߿���R3032���߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_hcmp_add_point", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_hcmp_add_point(ushort CardNo, ushort hcmp, int cmp_pos);
        //���ӱȽϵ�unit��������DMC5X10ϵ�����忨��E5032���߿���
        [DllImport("LTDMC.dll")]
        public static extern short dmc_hcmp_add_point_unit(ushort CardNo, ushort hcmp, double cmp_pos);       
        //���ö�ȡ����ģʽ�������������������忨��E3032���߿���R3032���߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_hcmp_set_liner", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_hcmp_set_liner(ushort CardNo, ushort hcmp, int Increment, int Count);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_hcmp_get_liner", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_hcmp_get_liner(ushort CardNo, ushort hcmp, ref int Increment, ref int Count);
        //��������ģʽ������������DMC5X10ϵ�����忨��E5032���߿���
        [DllImport("LTDMC.dll")]
        public static extern short dmc_hcmp_set_liner_unit(ushort CardNo, ushort hcmp, double Increment, int Count);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_hcmp_get_liner_unit(ushort CardNo, ushort hcmp, ref double Increment, ref int Count);
        //��ȡ���ٱȽ�״̬���������������忨��E3032���߿���R3032���߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_hcmp_get_current_state", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_hcmp_get_current_state(ushort CardNo, ushort hcmp, ref int remained_points, ref int current_point, ref int runned_points);
        //��ȡ���ٱȽ�״̬��������DMC5X10ϵ�����忨��E5032���߿���
        [DllImport("LTDMC.dll")]
        public static extern short dmc_hcmp_get_current_state_unit(ushort CardNo, ushort hcmp, ref int remained_points, ref double current_point, ref int runned_points); //��ȡ���ٱȽ�״̬
        //����Ƚϵ㣨�������������忨�����߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_hcmp_clear_points", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_hcmp_clear_points(ushort CardNo, ushort hcmp);
        //��ȡָ��CMP�˿ڵĵ�ƽ��������
        [DllImport("LTDMC.dll", EntryPoint = "dmc_read_cmp_pin", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_read_cmp_pin(ushort CardNo, ushort hcmp);
        //����cmp�˿������������
        [DllImport("LTDMC.dll", EntryPoint = "dmc_write_cmp_pin", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_write_cmp_pin(ushort CardNo, ushort hcmp, ushort on_off);
        //1��	���û��淽ʽ���ӱȽ�λ�ã���������DMC5000/5X10ϵ�����忨��
        [DllImport("LTDMC.dll")]
        public static extern short dmc_hcmp_fifo_set_mode(ushort CardNo, ushort hcmp, ushort fifo_mode);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_hcmp_fifo_get_mode(ushort CardNo, ushort hcmp, ref ushort fifo_mode);
        //2��	��ȡʣ�໺��״̬����λ��ͨ���˺����ж��Ƿ�������ӱȽ�λ�ã�������DMC5000/5X10ϵ�����忨��
        [DllImport("LTDMC.dll")]
        public static extern short dmc_hcmp_fifo_get_state(ushort CardNo, ushort hcmp, ref long remained_points);
        //3��	������ķ�ʽ�������ӱȽ�λ�ã�������DMC5000ϵ�����忨��
        [DllImport("LTDMC.dll")]
        public static extern short dmc_hcmp_fifo_add_point_unit(ushort CardNo, ushort hcmp, ushort num, double[] cmp_pos);
        //4��	����Ƚ�λ��,Ҳ���FPGA��λ��ͬ���������������DMC5000/5X10ϵ�����忨��
        [DllImport("LTDMC.dll")]
        public static extern short dmc_hcmp_fifo_clear_points(ushort CardNo, ushort hcmp);
        //���Ӵ����ݣ������һ��ʱ�䣬ָ������������ɣ�������DMC5000/5X10ϵ�����忨��
        [DllImport("LTDMC.dll")]
        public static extern short dmc_hcmp_fifo_add_table(ushort CardNo, ushort hcmp, ushort num, double[] cmp_pos);
        //һά���ٱȽϣ�����ģʽ���ӵıȽϵ�����˶����������������ݣ�������DMC5X10ϵ�����忨��
        [DllImport("LTDMC.dll")]
        public static extern short dmc_hcmp_fifo_add_point_dir_unit(ushort CardNo, ushort hcmp, ushort num, double[] cmp_pos, uint dir);
        //һά���ٱȽϣ�����ģʽ���ӵıȽϵ�����˶��������Ӵ������ݣ�������DMC5X10ϵ�����忨��
        [DllImport("LTDMC.dll")]
        public static extern short dmc_hcmp_fifo_add_table_dir(ushort CardNo, ushort hcmp, ushort num, double[] cmp_pos, uint dir);
        //----------- ��ά����λ�ñȽ�-----------------------        
        //���ö�ȡ���ٱȽ�ʹ�ܣ��������������忨��EtherCAT���߿���
        [DllImport("LTDMC.dll")]
        public static extern short dmc_hcmp_2d_set_enable(ushort CardNo, ushort hcmp, ushort cmp_enable);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_hcmp_2d_get_enable(ushort CardNo, ushort hcmp, ref ushort cmp_enable);
        //���ö�ȡ��ά���ٱȽ������������������忨��
        [DllImport("LTDMC.dll")]
        public static extern short dmc_hcmp_2d_set_config(ushort CardNo, ushort hcmp, ushort cmp_mode, ushort x_axis, ushort x_cmp_source, ushort y_axis, ushort y_cmp_source, int error, ushort cmp_logic, int time, ushort pwm_enable, double duty, int freq, ushort port_sel, ushort pwm_number);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_hcmp_2d_get_config(ushort CardNo, ushort hcmp, ref ushort cmp_mode, ref ushort x_axis, ref ushort x_cmp_source, ref ushort y_axis, ref ushort y_cmp_source, ref int error, ref ushort cmp_logic, ref int time, ref ushort pwm_enable, ref double duty, ref int freq, ref ushort port_sel, ref ushort pwm_number);
        //���ö�ȡ��ά���ٱȽ�����������DMC5X10ϵ�����忨��E5032���߿���
        [DllImport("LTDMC.dll")]
        public static extern short dmc_hcmp_2d_set_config_unit(ushort CardNo, ushort hcmp, ushort cmp_mode, ushort x_axis, ushort x_cmp_source, double x_cmp_error, ushort y_axis, ushort y_cmp_source, double y_cmp_error, ushort cmp_logic, int time);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_hcmp_2d_get_config_unit(ushort CardNo, ushort hcmp, ref ushort cmp_mode, ref ushort x_axis, ref ushort x_cmp_source, ref double x_cmp_error, ref ushort y_axis, ref ushort y_cmp_source, ref double y_cmp_error, ref ushort cmp_logic, ref int time);
        //���Ӷ�ά����λ�ñȽϵ㣨�������������忨��
        [DllImport("LTDMC.dll")]
        public static extern short dmc_hcmp_2d_add_point(ushort CardNo, ushort hcmp, int x_cmp_pos, int y_cmp_pos);
        //���Ӷ�ά����λ�ñȽϵ�unit��������DMC5X10ϵ�����忨��E5032���߿���
        [DllImport("LTDMC.dll")]
        public static extern short dmc_hcmp_2d_add_point_unit(ushort CardNo, ushort hcmp, double x_cmp_pos, double y_cmp_pos, ushort cmp_outbit);
        //��ȡ��ά���ٱȽϲ������������������忨��
        [DllImport("LTDMC.dll")]
        public static extern short dmc_hcmp_2d_get_current_state(ushort CardNo, ushort hcmp, ref int remained_points, ref int x_current_point, ref int y_current_point, ref int runned_points, ref ushort current_state);
        //��ȡ��ά���ٱȽϲ�����������DMC5X10ϵ�����忨��EtherCAT���߿���
        [DllImport("LTDMC.dll")]
        public static extern short dmc_hcmp_2d_get_current_state_unit(ushort CardNo, ushort hcmp, ref int remained_points, ref double x_current_point, ref double y_current_point, ref int runned_points, ref ushort current_state, ref ushort current_outbit); 
        //�����ά����λ�ñȽϵ㣨�������������忨��EtherCAT���߿���
        [DllImport("LTDMC.dll")]
        public static extern short dmc_hcmp_2d_clear_points(ushort CardNo, ushort hcmp);
        //ǿ�ƶ�ά���ٱȽ�������������������忨��EtherCAT���߿���
        [DllImport("LTDMC.dll")]
        public static extern short dmc_hcmp_2d_force_output(ushort CardNo, ushort hcmp, ushort cmp_outbit);
        //���ö�ȡ��ά�Ƚ�PWM���ģʽ��������DMC5X10ϵ�����忨��EtherCAT���߿���
        [DllImport("LTDMC.dll")]
        public static extern short dmc_hcmp_2d_set_pwmoutput(ushort CardNo, ushort hcmp, ushort pwm_enable, double duty, double freq, ushort pwm_number);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_hcmp_2d_get_pwmoutput(ushort CardNo, ushort hcmp, ref ushort pwm_enable, ref double duty, ref double freq, ref ushort pwm_number);
        
        //------------------------ͨ��IO-----------------------
        //��ȡ����ڵ�״̬���������������忨�����߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_read_inbit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_read_inbit(ushort CardNo, ushort bitno);
        //��������ڵ�״̬���������������忨�����߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_write_outbit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_write_outbit(ushort CardNo, ushort bitno, ushort on_off);
        //��ȡ����ڵ�״̬���������������忨�����߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_read_outbit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_read_outbit(ushort CardNo, ushort bitno);
        //��ȡ����˿ڵ�ֵ���������������忨�����߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_read_inport", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint dmc_read_inport(ushort CardNo, ushort portno);
        //��ȡ����˿ڵ�ֵ���������������忨�����߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_read_outport", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint dmc_read_outport(ushort CardNo, ushort portno);
        //������������˿ڵ�ֵ���������������忨�����߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_write_outport", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_write_outport(ushort CardNo, ushort portno, uint outport_val);
        //����ͨ������˿ڵ�ֵ��������
        [DllImport("LTDMC.dll", EntryPoint = "dmc_write_outport_16X", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_write_outport_16X(ushort CardNo, ushort portno, uint outport_val);
        //---------------------------ͨ��IO������ֵ���----------------------
        //��ȡ����ڵ�״̬��������DMC3000/5X10ϵ�����忨��
        [DllImport("LTDMC.dll")]
        public static extern short dmc_read_inbit_ex(ushort CardNo, ushort bitno, ref ushort state);
        //��ȡ����ڵ�״̬��������DMC3000/5X10ϵ�����忨��
        [DllImport("LTDMC.dll")]
        public static extern short dmc_read_outbit_ex(ushort CardNo, ushort bitno, ref ushort state);
        //��ȡ����˿ڵ�ֵ��������DMC3000/5X10ϵ�����忨��
        [DllImport("LTDMC.dll")]
        public static extern short dmc_read_inport_ex(ushort CardNo, ushort portno, ref uint state);
        //��ȡ����˿ڵ�ֵ��������DMC3000/5X10ϵ�����忨��
        [DllImport("LTDMC.dll")]
        public static extern short dmc_read_outport_ex(ushort CardNo, ushort portno, ref uint state);
        
        //���ö�ȡ����IOӳ���ϵ���������������忨�� 
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_io_map_virtual", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_io_map_virtual(ushort CardNo, ushort bitno, ushort MapIoType, ushort MapIoIndex, double Filter);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_io_map_virtual", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_io_map_virtual(ushort CardNo, ushort bitno, ref ushort MapIoType, ref ushort MapIoIndex, ref double Filter);
        //��ȡ��������ڵ�״̬���������������忨��
        [DllImport("LTDMC.dll", EntryPoint = "dmc_read_inbit_virtual", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_read_inbit_virtual(ushort CardNo, ushort bitno);
        //IO��ʱ��ת���������������忨�����߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_reverse_outbit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_reverse_outbit(ushort CardNo, ushort bitno, double reverse_time);
        //���ö�ȡIO����ģʽ���������������忨�����߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_io_count_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_io_count_mode(ushort CardNo, ushort bitno, ushort mode, double filter_time);        
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_io_count_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_io_count_mode(ushort CardNo, ushort bitno, ref ushort mode, ref double filter_time);
        //����IO����ֵ���������������忨�����߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_io_count_value", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_io_count_value(ushort CardNo, ushort bitno, uint CountValue);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_io_count_value", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_io_count_value(ushort CardNo, ushort bitno, ref uint CountValue);
                 
        //-----------------------ר��IO ���忨ר��-------------------------
        //���ö�ȡ��IOӳ���ϵ���������������忨�����߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_axis_io_map", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_axis_io_map(ushort CardNo, ushort Axis, ushort IoType, ushort MapIoType, ushort MapIoIndex, double Filter);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_axis_io_map", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_axis_io_map(ushort CardNo, ushort Axis, ushort IoType, ref ushort MapIoType, ref ushort MapIoIndex, ref double Filter);
        //��������ר��IO�˲�ʱ�䣨�������������忨��
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_special_input_filter", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_special_input_filter(ushort CardNo, double Filter);
        // ��ԭ������ź����ã�(DMC3410ר��)
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_sd_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_sd_mode(ushort CardNo, ushort axis, ushort sd_logic, ushort sd_mode);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_sd_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_sd_mode(ushort CardNo, ushort axis, ref ushort sd_logic, ref ushort sd_mode);
        //���ö�ȡINP�źţ��������������忨��
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_inp_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_inp_mode(ushort CardNo, ushort axis, ushort enable, ushort inp_logic);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_inp_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_inp_mode(ushort CardNo, ushort axis, ref ushort enable, ref ushort inp_logic);
        //���ö�ȡRDY�źţ�������
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_rdy_mode(ushort CardNo, ushort axis, ushort enable, ushort rdy_logic);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_rdy_mode(ushort CardNo, ushort axis, ref ushort enable, ref ushort rdy_logic);
        //���ö�ȡERC�źţ�������
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_erc_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_erc_mode(ushort CardNo, ushort axis, ushort enable, ushort erc_logic, ushort erc_width, ushort erc_off_time);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_erc_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_erc_mode(ushort CardNo, ushort axis, ref ushort enable, ref ushort erc_logic, ref ushort erc_width, ref ushort erc_off_time);
        //���ö�ȡALM�źţ��������������忨��
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_alm_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_alm_mode(ushort CardNo, ushort axis, ushort enable, ushort alm_logic, ushort alm_action);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_alm_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_alm_mode(ushort CardNo, ushort axis, ref ushort enable, ref ushort alm_logic, ref ushort alm_action);
        //���ö�ȡEZ�źţ��������������忨��
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_ez_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_ez_mode(ushort CardNo, ushort axis, ushort ez_logic, ushort ez_mode, double filter);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_ez_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_ez_mode(ushort CardNo, ushort axis, ref ushort ez_logic, ref ushort ez_mode, ref double filter);
        //�����ȡSEVON�źţ��������������忨��
        [DllImport("LTDMC.dll", EntryPoint = "dmc_write_sevon_pin", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_write_sevon_pin(ushort CardNo, ushort axis, ushort on_off);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_read_sevon_pin", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_read_sevon_pin(ushort CardNo, ushort axis);
        //����ERC�ź�������������������忨��
        [DllImport("LTDMC.dll", EntryPoint = "dmc_write_erc_pin", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_write_erc_pin(ushort CardNo, ushort axis, ushort sel);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_read_erc_pin", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_read_erc_pin(ushort CardNo, ushort axis);
        //��ȡRDY״̬���������������忨��
        [DllImport("LTDMC.dll", EntryPoint = "dmc_read_rdy_pin", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_read_rdy_pin(ushort CardNo, ushort axis);
        //����ŷ���λ�źţ�������
        [DllImport("LTDMC.dll")]
        public static extern short dmc_write_sevrst_pin(ushort CardNo, ushort axis, ushort on_off);
        //���ŷ���λ�źţ�������
        [DllImport("LTDMC.dll")]
        public static extern short dmc_read_sevrst_pin(ushort CardNo, ushort axis);

        //---------------------������ ���忨---------------------
        //�趨��ȡ�������ļ�����ʽ���������������忨��
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_counter_inmode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_counter_inmode(ushort CardNo, ushort axis, ushort mode);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_counter_inmode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_counter_inmode(ushort CardNo, ushort axis, ref ushort mode);
        //������ֵ���������������忨��
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_encoder", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int dmc_get_encoder(ushort CardNo, ushort axis);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_encoder", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_encoder(ushort CardNo, ushort axis, int encoder_value);
        //������ֵ(����)��������DMC5000/5X10ϵ�����忨���������߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_encoder_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_encoder_unit(ushort CardNo, ushort axis, double pos);     //��ǰ����λ��
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_encoder_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_encoder_unit(ushort CardNo, ushort axis, ref double pos);        
        //---------------------���������� ���߿�---------------------
        //���ֱ����������ã�ͬdmc_set_extra_encoder��
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_handwheel_encoder(ushort CardNo, ushort channel, int pos);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_handwheel_encoder(ushort CardNo, ushort channel, ref int pos);
        //���ø�������ģʽ���������������߿���
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_extra_encoder_mode(ushort CardNo, ushort channel, ushort inmode, ushort multi);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_extra_encoder_mode(ushort CardNo, ushort channel, ref ushort inmode, ref ushort multi);
        //���ø���������ֵ���������������߿���
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_extra_encoder(ushort CardNo, ushort channel, int pos);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_extra_encoder(ushort CardNo, ushort channel, ref int pos);
        //---------------------λ�ü�������---------------------
        //��ǰλ��(����)��������DMC5000/5X10ϵ�����忨���������߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_position_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_position_unit(ushort CardNo, ushort axis, double pos);   //��ǰָ��λ��
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_position_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_position_unit(ushort CardNo, ushort axis, ref double pos);
        //��ǰλ��(����)���������������忨��
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_position", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int dmc_get_position(ushort CardNo, ushort axis);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_position", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_position(ushort CardNo, ushort axis, int current_position);
        //--------------------�˶�״̬----------------------	
        //��ȡָ����ĵ�ǰ�ٶȣ��������������忨��
        [DllImport("LTDMC.dll", EntryPoint = "dmc_read_current_speed", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern double dmc_read_current_speed(ushort CardNo, ushort axis);
        //��ȡ��ǰ�ٶ�(����)��������DMC5000/5X10ϵ�����忨���������߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_read_current_speed_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_read_current_speed_unit(ushort CardNo, ushort Axis, ref double current_speed);   //�ᵱǰ�����ٶ�
        //��ȡ��ǰ���Ĳ岹�ٶȣ�������DMC5X10ϵ�����忨���������߿���
        [DllImport("LTDMC.dll")]
        public static extern short dmc_read_vector_speed_unit(ushort CardNo, ushort Crd, ref double current_speed);	//��ȡ��ǰ���Ĳ岹�ٶ�
        //��ȡָ�����Ŀ��λ�ã��������������忨��R3032���߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_target_position", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int dmc_get_target_position(ushort CardNo, ushort axis);
        //��ȡָ�����Ŀ��λ��(����)��������DMC5X10ϵ�����忨������EtherCAT����ϵ�п���
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_target_position_unit(ushort CardNo, ushort axis, ref double pos);
        //��ȡָ������˶�״̬���������������忨�����߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_check_done", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_check_done(ushort CardNo, ushort axis);
        //��ȡָ������˶�״̬�����������п���
        [DllImport("LTDMC.dll")]
        public static extern short dmc_check_done_ex(ushort CardNo, ushort axis, ref ushort state);
        //�岹�˶�״̬���������������忨�����߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_check_done_multicoor", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_check_done_multicoor(ushort CardNo, ushort crd);
        //��ȡָ�����й��˶��źŵ�״̬���������������忨�����߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_axis_io_status", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint dmc_axis_io_status(ushort CardNo, ushort axis);
        //��ȡָ�����й��˶��źŵ�״̬�����������п���
        [DllImport("LTDMC.dll")]
        public static extern short dmc_axis_io_status_ex(ushort CardNo, ushort axis, ref uint state);
        //����ֹͣ���������������忨�����߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_stop", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_stop(ushort CardNo, ushort axis, ushort stop_mode);
        //ֹͣ�岹�����������������忨�����߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_stop_multicoor", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_stop_multicoor(ushort CardNo, ushort crd, ushort stop_mode);
        //����ֹͣ�����ᣨ�������������忨�����߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_emg_stop", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_emg_stop(ushort CardNo);
        //���忨ָ�� ��������ߺ�ͨѶ״̬���������������忨��
        [DllImport("LTDMC.dll", EntryPoint = "dmc_LinkState", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_LinkState(ushort CardNo, ref ushort State);
        //��ȡָ������˶�ģʽ��������DMC5000/5X10ϵ�����忨���������߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_axis_run_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_axis_run_mode(ushort CardNo, ushort axis, ref ushort run_mode);  
        //��ȡ��ֹͣԭ���������������忨�����߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_stop_reason", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_stop_reason(ushort CardNo, ushort axis, ref int StopReason);    
        //�����ֹͣԭ���������������忨�����߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_clear_stop_reason", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_clear_stop_reason(ushort CardNo, ushort axis);
        //trace���ܣ��ڲ�ʹ�ú�����
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_trace", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_trace(ushort CardNo, ushort axis, ushort enable);   //trace����
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_trace", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_trace(ushort CardNo, ushort axis, ref ushort enable);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_read_trace_data", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_read_trace_data(ushort CardNo, ushort axis, ushort data_option, ref int ReceiveSize, double[] time, double[] data, ref int remain_num);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_trace_start(ushort CardNo, ushort AxisNum, ushort[] AxisList);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_trace_stop(ushort CardNo);

        //�������㣨���ã�
        [DllImport("LTDMC.dll", EntryPoint = "dmc_calculate_arclength_center", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_calculate_arclength_center(double[] start_pos, double[] target_pos, double[] cen_pos, ushort arc_dir, double circle, ref double ArcLength);      //����Բ��Բ������
        [DllImport("LTDMC.dll", EntryPoint = "dmc_calculate_arclength_3point", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_calculate_arclength_3point(double[] start_pos, double[] mid_pos, double[] target_pos, double circle, ref double ArcLength);      //��������Բ������
        [DllImport("LTDMC.dll", EntryPoint = "dmc_calculate_arclength_radius", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_calculate_arclength_radius(double[] start_pos, double[] target_pos, double arc_radius, ushort arc_dir, double circle, ref double ArcLength);     //����뾶Բ������

        //--------------------CAN-IO��չ----------------------	
        //CAN-IO��չ���ɽӿں�����������
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_can_state", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_can_state(ushort CardNo, ushort NodeNum, ushort state, ushort Baud);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_can_state", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_can_state(ushort CardNo, ref ushort NodeNum, ref ushort state);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_write_can_outbit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_write_can_outbit(ushort CardNo, ushort Node, ushort bitno, ushort on_off);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_read_can_outbit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_read_can_outbit(ushort CardNo, ushort Node, ushort bitno);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_read_can_inbit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_read_can_inbit(ushort CardNo, ushort Node, ushort bitno);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_write_can_outport", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_write_can_outport(ushort CardNo, ushort Node, ushort PortNo, uint outport_val);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_read_can_outport", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint dmc_read_can_outport(ushort CardNo, ushort Node, ushort PortNo);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_read_can_inport", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern uint dmc_read_can_inport(ushort CardNo, ushort Node, ushort PortNo);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_can_errcode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_can_errcode(ushort CardNo, ref ushort Errcode);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_can_errcode_extern(ushort CardNo, ref ushort Errcode, ref ushort msg_losed, ref ushort emg_msg_num, ref ushort lostHeartB, ref ushort EmgMsg);
        //����CAN io������������������忨��
        [DllImport("LTDMC.dll")]
        public static extern short nmc_write_outbit(ushort CardNo, ushort NodeID, ushort IoBit, ushort IoValue);
        //��ȡCAN io������������������忨��
        [DllImport("LTDMC.dll")]
        public static extern short nmc_read_outbit(ushort CardNo, ushort NodeID, ushort IoBit, ref ushort IoValue);
        //��ȡCAN io���루�������������忨��
        [DllImport("LTDMC.dll")]
        public static extern short nmc_read_inbit(ushort CardNo, ushort NodeID, ushort IoBit, ref ushort IoValue);
        //����CAN io���32λ���������������忨��
        [DllImport("LTDMC.dll")]
        public static extern short nmc_write_outport(ushort CardNo, ushort NodeID, ushort PortNo, uint IoValue);
        //��ȡCAN io���32λ���������������忨��
        [DllImport("LTDMC.dll")]
        public static extern short nmc_read_outport(ushort CardNo, ushort NodeID, ushort PortNo, ref uint IoValue);
        //��ȡCAN io����32λ���������������忨��
        [DllImport("LTDMC.dll")]
        public static extern short nmc_read_inport(ushort CardNo, ushort NodeID, ushort PortNo, ref uint IoValue);
        //---------------------------CAN IO������ֵ���----------------------
        //����CAN io�����������DMC3000/5X10ϵ�����忨��
        [DllImport("LTDMC.dll")]
        public static extern short nmc_write_outbit_ex(ushort CardNo, ushort NoteID, ushort IoBit, ushort IoValue, ref ushort state);
        //��ȡCAN io�����������DMC3000/5X10ϵ�����忨��
        [DllImport("LTDMC.dll")]
        public static extern short nmc_read_outbit_ex(ushort CardNo, ushort NoteID, ushort IoBit, ref ushort IoValue, ref ushort state);
        //��ȡCAN io���루������DMC3000/5X10ϵ�����忨��
        [DllImport("LTDMC.dll")]
        public static extern short nmc_read_inbit_ex(ushort CardNo, ushort NoteID, ushort IoBit, ref ushort IoValue, ref ushort state);
        //����CAN io���32λ��������DMC3000/5X10ϵ�����忨��
        [DllImport("LTDMC.dll")]
        public static extern short nmc_write_outport_ex(ushort CardNo, ushort NoteID, ushort portno, uint outport_val, ref ushort state);
        //��ȡCAN io���32λ��������DMC3000/5X10ϵ�����忨��
        [DllImport("LTDMC.dll")]
        public static extern short nmc_read_outport_ex(ushort CardNo, ushort NoteID, ushort portno, ref uint outport_val, ref ushort state);
        //��ȡCAN io����32λ��������DMC3000/5X10ϵ�����忨��
        [DllImport("LTDMC.dll")]
        public static extern short nmc_read_inport_ex(ushort CardNo, ushort NoteID, ushort portno, ref uint inport_val, ref ushort state);
        //---------------------------CAN ADDA----------------------
        //CAN ADDAָ�� ����DA���� ���������������忨��
        [DllImport("LTDMC.dll")]
        public static extern short nmc_set_da_output(ushort CardNo, ushort NoteID, ushort channel, double Value);
        //��ȡCAN DA�������������������忨��
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_da_output(ushort CardNo, ushort NoteID, ushort channel, ref double Value);
        //��ȡCAN AD�������������������忨��
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_ad_input(ushort CardNo, ushort NoteID, ushort channel, ref double Value);
        //����CAN ADģʽ���������������忨��
        [DllImport("LTDMC.dll")]
        public static extern short nmc_set_ad_mode(ushort CardNo, ushort NoteID, ushort channel, ushort mode, uint buffer_nums);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_ad_mode(ushort CardNo, ushort NoteID, ushort channel, ref ushort mode, uint buffer_nums);
        //����CAN DAģʽ���������������忨��
        [DllImport("LTDMC.dll")]
        public static extern short nmc_set_da_mode(ushort CardNo, ushort NoteID, ushort channel, ushort mode, uint buffer_nums);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_da_mode(ushort CardNo, ushort NoteID, ushort channel, ref ushort mode, uint buffer_nums);
        //CAN����д��flash���������������忨��
        [DllImport("LTDMC.dll")]
        public static extern short nmc_write_to_flash(ushort CardNo, ushort PortNum, ushort NodeNum);
        //CAN�������ӣ��������������忨��
        [DllImport("LTDMC.dll")]
        public static extern short nmc_set_connect_state(ushort CardNo, ushort NodeNum, ushort state, ushort baud);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_connect_state(ushort CardNo, ref ushort NodeNum, ref ushort state);
        //---------------------------CAN ADDA������ֵ���----------------------
        //����CAN DA������������DMC3000/5X10ϵ�����忨��
        [DllImport("LTDMC.dll")]
        public static extern short nmc_set_da_output_ex(ushort CardNo, ushort NoteID, ushort channel, double Value, ref ushort state);
        //��ȡCAN DA������������DMC3000/5X10ϵ�����忨��
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_da_output_ex(ushort CardNo, ushort NoteID, ushort channel, ref double Value, ref ushort state);
        //��ȡCAN AD������������DMC3000/5X10ϵ�����忨��
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_ad_input_ex(ushort CardNo, ushort NoteID, ushort channel, ref double Value, ref ushort state);
        //����CAN ADģʽ��������DMC3000/5X10ϵ�����忨��
        [DllImport("LTDMC.dll")]
        public static extern short nmc_set_ad_mode_ex(ushort CardNo, ushort NoteID, ushort channel, ushort mode, uint buffer_nums, ref ushort state);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_ad_mode_ex(ushort CardNo, ushort NoteID, ushort channel, ref ushort mode, uint buffer_nums, ref ushort state);
        //����CAN DAģʽ��������DMC3000/5X10ϵ�����忨��
        [DllImport("LTDMC.dll")]
        public static extern short nmc_set_da_mode_ex(ushort CardNo, ushort NoteID, ushort channel, ushort mode, uint buffer_nums, ref ushort state);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_da_mode_ex(ushort CardNo, ushort NoteID, ushort channel, ref ushort mode, uint buffer_nums, ref ushort state);
        //����д��flash��������DMC3000/5X10ϵ�����忨��
        [DllImport("LTDMC.dll")]
        public static extern short nmc_write_to_flash_ex(ushort CardNo, ushort PortNum, ushort NodeNum, ref ushort state);

        //--------------------�����岹����----------------------	
        //��������������������DMC5000/5X10ϵ�����忨��E5032���߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_open_list", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_open_list(ushort CardNo, ushort Crd, ushort AxisNum, ushort[] AxisList);
        //�ر�������������������DMC5000/5X10ϵ�����忨��E5032���߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_close_list", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_close_list(ushort CardNo, ushort Crd);
        //��λ������������Ԥ����
        [DllImport("LTDMC.dll")]
        public static extern short dmc_conti_reset_list(ushort CardNo, ushort Crd);
        //�����岹��ֹͣ��������DMC5000/5X10ϵ�����忨��E5032���߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_stop_list", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_stop_list(ushort CardNo, ushort Crd, ushort stop_mode);
        //�����岹����ͣ��������DMC5000/5X10ϵ�����忨��E5032���߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_pause_list", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_pause_list(ushort CardNo, ushort Crd);
        //��ʼ�����岹��������DMC5000/5X10ϵ�����忨��E5032���߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_start_list", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_start_list(ushort CardNo, ushort Crd);
        //��������岹�˶�״̬��0-���У�1-��ͣ��2-����ֹͣ��DMC5X10��֧�֣���3-δ������4-���У�������DMC5000/5X10ϵ�����忨��E5032���߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_get_run_state", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_get_run_state(ushort CardNo, ushort Crd);
        //��������岹�˶�״̬��0-���У�1-ֹͣ��Ԥ����
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_check_done", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_check_done(ushort CardNo, ushort Crd);  
        //�������岹ʣ�໺������������DMC5000/5X10ϵ�����忨��E5032���߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_remain_space", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int dmc_conti_remain_space(ushort CardNo, ushort Crd);
        //��ȡ��ǰ�����岹�εı�ţ�������DMC5000/5X10ϵ�����忨��E5032���߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_read_current_mark", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int dmc_conti_read_current_mark(ushort CardNo, ushort Crd);
        //blend�սǹ���ģʽ��������DMC5000ϵ�����忨��
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_set_blend", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_set_blend(ushort CardNo, ushort Crd, ushort enable);      
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_get_blend", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_get_blend(ushort CardNo, ushort Crd, ref ushort enable);
        //����ÿ���ٶȱ���  ������ָ�������DMC5000/5X10ϵ�����忨��E5032���߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_set_override", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_set_override(ushort CardNo, ushort Crd, double Percent);      
        //���ò岹�ж�̬���٣�������DMC5000/5X10ϵ�����忨��
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_change_speed_ratio", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_change_speed_ratio(ushort CardNo, ushort Crd, double Percent);
        //С�߶�ǰհ��������DMC5000/5X10ϵ�����忨��E5032���߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_set_lookahead_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_set_lookahead_mode(ushort CardNo, ushort Crd, ushort enable, int LookaheadSegments, double PathError, double LookaheadAcc);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_get_lookahead_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_get_lookahead_mode(ushort CardNo, ushort Crd, ref ushort enable, ref int LookaheadSegments, ref double PathError, ref double LookaheadAcc);
        //--------------------�����岹IO����----------------------
        //�ȴ�IO���루������DMC5000/5X10ϵ�����忨��E5032���߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_wait_input", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_wait_input(ushort CardNo, ushort Crd, ushort bitno, ushort on_off, double TimeOut, int mark);
        //����ڹ켣���IO�ͺ������������DMC5000/5X10ϵ�����忨��E5032���߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_delay_outbit_to_start", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_delay_outbit_to_start(ushort CardNo, ushort Crd, ushort bitno, ushort on_off, double delay_value, ushort delay_mode, double ReverseTime);      
        //����ڹ켣�յ�IO�ͺ������������DMC5000/5X10ϵ�����忨��E5032���߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_delay_outbit_to_stop", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_delay_outbit_to_stop(ushort CardNo, ushort Crd, ushort bitno, ushort on_off, double delay_time, double ReverseTime);      
        //����ڹ켣�յ�IO��ǰ�����������DMC5000/5X10ϵ�����忨��E5032���߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_ahead_outbit_to_stop", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_ahead_outbit_to_stop(ushort CardNo, ushort Crd, ushort bitno, ushort on_off, double ahead_value, ushort ahead_mode, double ReverseTime);  
        //�����岹��ȷλ��CMP�����������DMC5000/5X10ϵ�����忨��E5032���߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_accurate_outbit_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_accurate_outbit_unit(ushort CardNo, ushort Crd, ushort cmp_no, ushort on_off, ushort map_axis, double abs_pos, ushort pos_source, double ReverseTime);    
        //�����岹����IO�����������DMC5000/5X10ϵ�����忨��E5032���߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_write_outbit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_write_outbit(ushort CardNo, ushort Crd, ushort bitno, ushort on_off, double ReverseTime);     
        //�������δִ�����IO��������DMC5000/5X10ϵ�����忨��E5032���߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_clear_io_action", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_clear_io_action(ushort CardNo, ushort Crd, uint IoMask);    
        //�����岹��ͣ���쳣ʱIO���״̬��������DMC5000/5X10ϵ�����忨��E5032���߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_set_pause_output", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_set_pause_output(ushort CardNo, ushort Crd, ushort action, int mask, int state);     //��ͣʱIO��� action 0, ��������1�� ��ͣʱ���io_state; 2 ��ͣʱ���io_state, ��������ʱ���Ȼָ�ԭ����io; 3,��2�Ļ����ϣ�ֹͣʱҲ��Ч��
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_get_pause_output", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_get_pause_output(ushort CardNo, ushort Crd, ref ushort action, ref int mask, ref int state);
        //��ʱָ�������DMC5000/5X10ϵ�����忨��E5032���߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_delay", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_delay(ushort CardNo, ushort Crd, double delay_time, int mark);     //������ʱָ��
        //IO�����ʱ��ת��������DMC5000/5X10ϵ�����忨��E5032���߿���
        [DllImport("LTDMC.dll")]
        public static extern short dmc_conti_reverse_outbit(ushort CardNo, ushort Crd, ushort bitno, double reverse_time);
        //IO��ʱ�����������DMC5000/5X10ϵ�����忨��E5032���߿���
        [DllImport("LTDMC.dll")]
        public static extern short dmc_conti_delay_outbit(ushort CardNo, ushort Crd, ushort bitno, ushort on_off, double delay_time);
        //�����岹�����˶���������DMC5000/5X10ϵ�����忨��E5032���߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_pmove_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_pmove_unit(ushort CardNo, ushort Crd, ushort Axis, double dist, ushort posi_mode, ushort mode, int mark); //�����岹�п���ָ�������˶�
        //�����岹ֱ�߲岹��������DMC5000/5X10ϵ�����忨��E5032���߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_line_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_line_unit(ushort CardNo, ushort Crd, ushort AxisNum, ushort[] AxisList, double[] Target_Pos, ushort posi_mode, int mark); //�����岹ֱ��
        //�����岹Բ��Բ���岹��������DMC5000/5X10ϵ�����忨��E5032���߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_arc_move_center_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_arc_move_center_unit(ushort CardNo, ushort Crd, ushort AxisNum, ushort[] AxisList, double[] Target_Pos, double[] Cen_Pos, ushort Arc_Dir, int Circle, ushort posi_mode, int mark);    
        //�����岹�뾶Բ���岹��������DMC5000/5X10ϵ�����忨��E5032���߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_arc_move_radius_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_arc_move_radius_unit(ushort CardNo, ushort Crd, ushort AxisNum, ushort[] AxisList, double[] Target_Pos, double Arc_Radius, ushort Arc_Dir, int Circle, ushort posi_mode, int mark);   
        //�����岹3��Բ���岹��������DMC5000/5X10ϵ�����忨��E5032���߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_arc_move_3points_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_arc_move_3points_unit(ushort CardNo, ushort Crd, ushort AxisNum, ushort[] AxisList, double[] Target_Pos, double[] Mid_Pos, int Circle, ushort posi_mode, int mark);     
        //�����岹���β岹��������DMC5000/5X10ϵ�����忨��E5032���߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_rectangle_move_unit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_rectangle_move_unit(ushort CardNo, ushort Crd, ushort AxisNum, ushort[] AxisList, double[] TargetPos, double[] MaskPos, int Count, ushort rect_mode, ushort posi_mode, int mark);     
        //���������߲岹�˶�ģʽ��������DMC5000/5X10ϵ�����忨��E5032���߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_set_involute_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_set_involute_mode(ushort CardNo, ushort Crd, ushort mode);      //�����������Ƿ���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_get_involute_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_get_involute_mode(ushort CardNo, ushort Crd, ref ushort mode);   //��ȡ�������Ƿ�������
        //�����ã�
        [DllImport("LTDMC.dll")]
        public static extern short dmc_conti_line_unit_extern(ushort CardNo, ushort Crd, ushort AxisNum, ushort[] AxisList, double[] Target_Pos, double[] Cen_Pos, ushort posi_mode, int mark);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_conti_arc_move_center_unit_extern(ushort CardNo, ushort Crd, ushort AxisNum, ushort[] AxisList, double[] Target_Pos, double[] Cen_Pos, double Arc_Radius, ushort posi_mode, int mark);
        //���ö�ȡ���Ÿ���ģʽ��������DMC5000/5X10ϵ�����忨��E5032���߿���
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_gear_follow_profile(ushort CardNo, ushort axis, ushort enable, ushort master_axis, double ratio);//˫Z��
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_gear_follow_profile(ushort CardNo, ushort axis, ref ushort enable, ref ushort master_axis, ref double ratio);
              
        //--------------------PWM����----------------------
        //PWM���ƣ����ã�
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_pwm_pin(ushort CardNo, ushort portno, ushort ON_OFF, double dfreqency, double dduty);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_pwm_pin(ushort CardNo, ushort portno, ref ushort ON_OFF, ref double dfreqency, ref double dduty);
        //���ö�ȡPWMʹ�ܣ�������DMC5000/5X10ϵ�����忨��E5032���߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_pwm_enable", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_pwm_enable(ushort CardNo, ushort enable);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_pwm_enable", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_pwm_enable(ushort CardNo, ref ushort enable);
        //���ö�ȡPWM���������������DMC5000/5X10ϵ�����忨��E5032���߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_pwm_output", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_pwm_output(ushort CardNo, ushort pwm_no, double fDuty, double fFre);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_pwm_output", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_pwm_output(ushort CardNo, ushort pwm_no, ref double fDuty, ref double fFre);        
        //�����岹PWM�����������DMC5000/5X10ϵ�����忨��E5032���߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_set_pwm_output", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_set_pwm_output(ushort CardNo, ushort Crd, ushort pwm_no, double fDuty, double fFre);
        //����PWM���ܣ����ã�
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_pwm_enable_extern(ushort CardNo, ushort channel, ushort enable);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_pwm_enable_extern(ushort CardNo, ushort channel, ref ushort enable);
        //����PWM���ض�Ӧ��ռ�ձȣ�������DMC5000/5X10ϵ�����忨��E5032���߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_pwm_onoff_duty", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_pwm_onoff_duty(ushort CardNo, ushort PwmNo, double fOnDuty, double fOffDuty);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_pwm_onoff_duty", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_pwm_onoff_duty(ushort CardNo, ushort PwmNo, ref double fOnDuty, ref double fOffDuty);
        //�����岹PWM�ٶȸ��棨������DMC5000/5X10ϵ�����忨��E5032���߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_set_pwm_follow_speed", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_set_pwm_follow_speed(ushort CardNo, ushort Crd, ushort pwm_no, ushort mode, double MaxVel, double MaxValue, double OutValue);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_get_pwm_follow_speed", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_get_pwm_follow_speed(ushort CardNo, ushort Crd, ushort pwm_no, ref ushort mode, ref double MaxVel, ref double MaxValue, ref double OutValue);
        //�����岹��Թ켣���PWM�ͺ������������DMC5000/5X10ϵ�����忨��E5032���߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_delay_pwm_to_start", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_delay_pwm_to_start(ushort CardNo, ushort Crd, ushort pwmno, ushort on_off, double delay_value, ushort delay_mode, double ReverseTime);
        //�����岹��Թ켣�յ�PWM��ǰ�����������DMC5000/5X10ϵ�����忨��E5032���߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_ahead_pwm_to_stop", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_ahead_pwm_to_stop(ushort CardNo, ushort Crd, ushort pwmno, ushort on_off, double ahead_value, ushort ahead_mode, double ReverseTime);
        //�����岹PWM���������������DMC5000/5X10ϵ�����忨��E5032���߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_write_pwm", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_write_pwm(ushort CardNo, ushort Crd, ushort pwmno, ushort on_off, double ReverseTime);

        //--------------------ADDA���----------------------
        //���ƿ����ߺ�DA���������DA���ʹ�ܣ��������������忨��
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_da_enable", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_da_enable(ushort CardNo, ushort enable);      
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_da_enable", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_da_enable(ushort CardNo, ref ushort enable);
        //����DA������������������忨��
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_da_output", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_da_output(ushort CardNo, ushort channel, double Vout);   
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_da_output", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_da_output(ushort CardNo, ushort channel, ref double Vout);
        //���ƿ����ߺ�AD���룬��ȡAD���루�������������忨��
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_ad_input(ushort CardNo, ushort channel, ref double Vout);
        //��������DAʹ�ܣ�������DMC5000/5X10ϵ�����忨��
        [DllImport("LTDMC.dll")]
        public static extern short dmc_conti_set_da_output(ushort CardNo, ushort Crd, ushort channel, double Vout);
        //��������DAʹ�ܣ�������DMC5000/5X10ϵ�����忨��
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_set_da_enable", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_set_da_enable(ushort CardNo, ushort Crd, ushort enable, ushort channel, int mark);
        //������da���棨Ԥ����
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_encoder_da_follow_enable", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_encoder_da_follow_enable(ushort CardNo, ushort axis, ushort enable);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_encoder_da_follow_enable", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_encoder_da_follow_enable(ushort CardNo, ushort axis, ref ushort enable);
        //�����岹DA�ٶȸ��棨������DMC5000/5X10ϵ�����忨��
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_set_da_follow_speed", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_set_da_follow_speed(ushort CardNo, ushort Crd, ushort da_no, double MaxVel, double MaxValue, double acc_offset, double dec_offset, double acc_dist, double dec_dist);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_conti_get_da_follow_speed", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_conti_get_da_follow_speed(ushort CardNo, ushort Crd, ushort da_no, ref double MaxVel, ref double MaxValue, ref double acc_offset, ref double dec_offset, ref double acc_dist, ref double dec_dist);

        //СԲ����ʹ�ܣ�������DMC5000/5X10ϵ�����忨��E5032���߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_arc_limit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_arc_limit(ushort CardNo, ushort Crd, ushort Enable, double MaxCenAcc, double MaxArcError);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_arc_limit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_arc_limit(ushort CardNo, ushort Crd, ref ushort Enable, ref double MaxCenAcc, ref double MaxArcError);
        //��Ԥ����
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_IoFilter(ushort CardNo, ushort bitno, double filter);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_IoFilter(ushort CardNo, ushort bitno, ref double filter);
        //�ݾಹ������ָ���ʹ�ã�
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_lsc_index_value(ushort CardNo, ushort axis, ushort IndexID, int IndexValue);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_lsc_index_value(ushort CardNo, ushort axis, ushort IndexID, ref int IndexValue);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_lsc_config(ushort CardNo, ushort axis, ushort Origin, uint Interal, uint NegIndex, uint PosIndex, double Ratio);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_lsc_config(ushort CardNo, ushort axis, ref ushort Origin, ref uint Interal, ref uint NegIndex, ref uint PosIndex, ref double Ratio);
        //���Ź���ָ���ʹ��
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_watchdog(ushort CardNo, ushort enable, uint time);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_call_watchdog(ushort CardNo);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_read_diagnoseData(ushort CardNo);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_conti_set_cmd_end(ushort CardNo, ushort Crd, ushort enable);
        //��������λ��������
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_zone_limit_config(ushort CardNo, ushort[] axis, ushort[] Source, int x_pos_p, int x_pos_n, int y_pos_p, int y_pos_n, ushort action_para);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_zone_limit_config(ushort CardNo, ushort[] axis, ushort[] Source, ref int x_pos_p, ref int x_pos_n, ref int y_pos_p, ref int y_pos_n, ref ushort action_para);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_zone_limit_enable(ushort CardNo, ushort enable);
        //�ụ�����ܣ�������
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_interlock_config(ushort CardNo, ushort[] axis, ushort[] Source, int delta_pos, ushort action_para);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_interlock_config(ushort CardNo, ushort[] axis, ushort[] Source, ref int delta_pos, ref ushort action_para);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_interlock_enable(ushort CardNo, ushort enable);
        //����ģʽ��������������DMC5000/5X10ϵ�����忨��
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_grant_error_protect(ushort CardNo, ushort axis, ushort enable, uint dstp_error, uint emg_error);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_grant_error_protect(ushort CardNo, ushort axis, ref ushort enable, ref uint dstp_error, ref uint emg_error);
        //����ģʽ����������������������DMC5000/5X10ϵ�����忨��
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_grant_error_protect_unit(ushort CardNo, ushort axis, ushort enable, double dstp_error, double emg_error);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_grant_error_protect_unit(ushort CardNo, ushort axis, ref ushort enable, ref double dstp_error, ref double emg_error);

        //����ּ��� ���ּ�̼�ר�ã�
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_camerablow_config(ushort CardNo, ushort camerablow_en, int cameraPos, ushort piece_num, int piece_distance, ushort axis_sel, int latch_distance_min);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_camerablow_config(ushort CardNo, ref ushort camerablow_en, ref int cameraPos, ref ushort piece_num, ref int piece_distance, ref ushort axis_sel, ref int latch_distance_min);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_clear_camerablow_errorcode(ushort CardNo);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_camerablow_errorcode(ushort CardNo, ref ushort errorcode);
        //����ͨ�����루0~15����Ϊ�����λ�źţ�������
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_io_limit_config(ushort CardNo, ushort portno, ushort enable, ushort axis_sel, ushort el_mode, ushort el_logic);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_io_limit_config(ushort CardNo, ushort portno, ref ushort enable, ref ushort axis_sel, ref ushort el_mode, ref ushort el_logic);
        //�����˲�������������
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_handwheel_filter(ushort CardNo, ushort axis, double filter_factor);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_handwheel_filter(ushort CardNo, ushort axis, ref double filter_factor);
        //��ȡ����ϵ����ĵ�ǰ�滮���꣨������
        [DllImport("LTDMC.dll")]
        public static extern short dmc_conti_get_interp_map(ushort CardNo, ushort Crd, ref ushort AxisNum, ushort[] AxisList, double[] pPosList);
        //����ϵ������� ��������
        [DllImport("LTDMC.dll")]
        public static extern short dmc_conti_get_crd_errcode(ushort CardNo, ushort Crd, ref ushort errcode);
        //����
        [DllImport("LTDMC.dll")]
        public static extern short dmc_line_unit_follow(ushort CardNo, ushort Crd, ushort AxisNum, ushort[] AxisList, double[] Dist, ushort posi_mode);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_conti_line_unit_follow(ushort CardNo, ushort Crd, ushort AxisNum, ushort[] AxisList, double[] pPosList, ushort posi_mode, int mark);
        //�����岹������DA������������
        [DllImport("LTDMC.dll")]
        public static extern short dmc_conti_set_da_action(ushort CardNo, ushort Crd, ushort mode, ushort portno, double dvalue);
        //���������ٶȣ�������
        [DllImport("LTDMC.dll")]
        public static extern short dmc_read_encoder_speed(ushort CardNo, ushort Axis, ref double current_speed);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_axis_follow_line_enable(ushort CardNo, ushort Crd, ushort enable_flag);
        //�岹�����岹����������
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_interp_compensation(ushort CardNo, ushort axis, double dvalue, double time);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_interp_compensation(ushort CardNo, ushort axis, ref double dvalue, ref double time);
        //��ȡ��������ľ��루������
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_distance_to_start(ushort CardNo, ushort Crd, ref double distance_x, ref double distance_y, int imark);
        //���ñ�־λ ��ʾ�Ƿ�ʼ���������㣨������
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_start_distance_flag(ushort CardNo, ushort Crd, ushort flag);

        //������棨������DMC5000/5X10ϵ�����忨��E5032���߿���
        [DllImport("LTDMC.dll")]
        public static extern short dmc_conti_gear_unit(ushort CardNo, ushort Crd, ushort axis, double dist, ushort follow_mode, int imark);
        //�켣���ʹ�����ã�������
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_path_fitting_enable(ushort CardNo, ushort Crd, ushort enable);
        //--------------------�ݾಹ��----------------------
        //�ݾಹ������(��)���������������忨��E5032���߿���
        [DllImport("LTDMC.dll")]
        public static extern short dmc_enable_leadscrew_comp(ushort CardNo, ushort axis, ushort enable);
        //�����߼��������������壩���������������忨��
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_leadscrew_comp_config(ushort CardNo, ushort axis, ushort n, int startpos, int lenpos, int[] pCompPos, int[] pCompNeg);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_leadscrew_comp_config(ushort CardNo, ushort axis, ref ushort n, ref int startpos, ref int lenpos, int[] pCompPos, int[] pCompNeg);
        //�����߼�������������������������DMC5X10ϵ�����忨��E5032���߿���
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_leadscrew_comp_config_unit(ushort CardNo, ushort axis, ushort n, double startpos, double lenpos, double[] pCompPos, double[] pCompNeg);       
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_leadscrew_comp_config_unit(ushort CardNo, ushort axis, ref ushort n, ref double startpos, ref double lenpos, double[] pCompPos, double[] pCompNeg);
        //�ݾಹ��ǰ������λ�ã�������λ��//20191025��������DMC3000ϵ�����忨��
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_position_ex(ushort CardNo, ushort axis, ref double pos);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_encoder_ex(ushort CardNo, ushort axis, ref double pos);
        //�ݾಹ��ǰ������λ�ã�������λ�� ������������DMC5X10ϵ�����忨��E5032���߿���
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_position_ex_unit(ushort CardNo, ushort axis, ref double pos);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_encoder_ex_unit(ushort CardNo, ushort axis, ref double pos);

        //ָ����������λ���˶� ���̶������˶���������DMC3000/5X10ϵ�����忨��
        [DllImport("LTDMC.dll")]
        public static extern short dmc_t_pmove_extern(ushort CardNo, ushort axis, double MidPos, double TargetPos, double Min_Vel, double Max_Vel, double stop_Vel, double acc, double dec, ushort posi_mode);
        //
        [DllImport("LTDMC.dll")]
        public static extern short dmc_t_pmove_extern_unit(ushort CardNo, ushort axis, double MidPos, double TargetPos, double Min_Vel, double Max_Vel, double stop_Vel, double acc, double dec, ushort posi_mode);
        //�����������ֵ�ͱ���������ֵ֮���ֵ�ı�����ֵ��������DMC5000/5X10ϵ�����忨��
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_pulse_encoder_count_error(ushort CardNo, ushort axis, ushort error);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_pulse_encoder_count_error(ushort CardNo, ushort axis, ref ushort error);
        //����������ֵ�ͱ���������ֵ֮���ֵ�Ƿ񳬹�������ֵ��������DMC5000/5X10ϵ�����忨��
        [DllImport("LTDMC.dll")]
        public static extern short dmc_check_pulse_encoder_count_error(ushort CardNo, ushort axis, ref int pulse_position, ref int enc_position);
        //����/�ض��������ֵ�ͱ���������ֵ֮���ֵ�ı�����ֵunit��������DMC5X10���忨��EtherCAT���߿���
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_pulse_encoder_count_error_unit(ushort CardNo, ushort axis, double error);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_pulse_encoder_count_error_unit(ushort CardNo, ushort axis, ref double error);
        //����������ֵ�ͱ���������ֵ֮���ֵ�Ƿ񳬹�������ֵunit��������DMC5X10���忨��EtherCAT���߿���
        [DllImport("LTDMC.dll")]
        public static extern short dmc_check_pulse_encoder_count_error_unit(ushort CardNo, ushort axis, ref double pulse_position, ref double enc_position);
        //ʹ�ܺ����ø��ٱ��������ڷ�Χ��ʱ���ֹͣģʽ��������DMC5000/5X10ϵ�����忨��
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_encoder_count_error_action_config(ushort CardNo, ushort enable, ushort stopmode);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_encoder_count_error_action_config(ushort CardNo, ref ushort enable, ref ushort stopmode);
        
        //������ּ��� �ּ�̼�ר��
        [DllImport("LTDMC.dll")]
        public static extern short dmc_sorting_close(ushort CardNo);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_sorting_start(ushort CardNo);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_sorting_set_init_config(ushort CardNo, ushort cameraCount, int[] pCameraPos, ushort[] pCamIONo, uint cameraTime, ushort cameraTrigLevel, ushort blowCount, int[] pBlowPos, ushort[] pBlowIONo, uint blowTime, ushort blowTrigLevel, ushort axis, ushort dir, ushort checkMode);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_sorting_set_camera_trig_count(ushort CardNo, ushort cameraNum, uint cameraTrigCnt);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_sorting_get_camera_trig_count(ushort CardNo, ushort cameraNum, ref uint pCameraTrigCnt, ushort count);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_sorting_set_blow_trig_count(ushort CardNo, ushort blowNum, uint blowTrigCnt);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_sorting_get_blow_trig_count(ushort CardNo, ushort blowNum, ref uint pBlowTrigCnt, ushort count);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_sorting_get_camera_config(ushort CardNo, ushort index, ref int pos, ref uint trigTime, ref ushort ioNo, ref ushort trigLevel);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_sorting_get_blow_config(ushort CardNo, ushort index, ref int pos, ref uint trigTime, ref ushort ioNo, ref ushort trigLevel);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_sorting_get_blow_status(ushort CardNo, ref int trigCntAll, ref ushort trigMore, ref ushort trigLess);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_sorting_trig_blow(ushort CardNo, ushort blowNum);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_sorting_set_blow_enable(ushort CardNo, ushort blowNum, ushort enable);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_sorting_set_piece_config(ushort CardNo, uint maxWidth, uint minWidth, uint minDistance, uint minTimeIntervel);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_sorting_get_piece_status(ushort CardNo, ref uint pieceFind, ref uint piecePassCam, ref uint dist2next, ref uint pieceWidth);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_sorting_set_cam_trig_phase(ushort CardNo, ushort blowNo, double coef);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_sorting_set_blow_trig_phase(ushort CardNo, ushort blowNo, double coef);
        
        //�ڲ�ʹ�ã�������
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_sevon_enable(ushort CardNo, ushort axis, ushort on_off);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_sevon_enable(ushort CardNo, ushort axis);

        //����������da���棨������DMC5000/5X10ϵ�����忨��
        [DllImport("LTDMC.dll")]
        public static extern short dmc_conti_set_encoder_da_follow_enable(ushort CardNo, ushort Crd, ushort axis, ushort enable);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_conti_get_encoder_da_follow_enable(ushort CardNo, ushort Crd, ref ushort axis, ref ushort enable);
        //����λ���������������������忨�����߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_set_factor_error", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_set_factor_error(ushort CardNo, ushort axis, double factor, int error);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_get_factor_error", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_get_factor_error(ushort CardNo, ushort axis, ref double factor, ref int error);
        //����/�ض�λ������unit��������DMC5X10���忨��EtherCAT���߿���
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_factor_error_unit(ushort CardNo, ushort axis, double factor, double error);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_factor_error_unit(ushort CardNo, ushort axis, ref double factor, ref double error);
        //����
        [DllImport("LTDMC.dll")]
        public static extern short dmc_check_done_pos(ushort CardNo, ushort axis, ushort posi_mode);
        //����
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_factor(ushort CardNo, ushort axis, double factor);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_error(ushort CardNo, ushort axis, int error);
        //���ָ�λ���������������忨�����߿���
        [DllImport("LTDMC.dll", EntryPoint = "dmc_check_success_pulse", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_check_success_pulse(ushort CardNo, ushort axis);
        [DllImport("LTDMC.dll", EntryPoint = "dmc_check_success_encoder", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short dmc_check_success_encoder(ushort CardNo, ushort axis);

        //IO���������������ܣ�������
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_io_count_profile(ushort CardNo, ushort chan, ushort bitno, ushort mode,double filter, double count_value, ushort[] axis_list, ushort axis_num, ushort stop_mode );
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_io_count_profile(ushort CardNo, ushort chan, ref ushort bitno, ref ushort mode,ref double filter, ref double count_value, ushort[] axis_list, ref ushort axis_num, ref ushort stop_mode );
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_io_count_enable(ushort CardNo, ushort chan, ushort ifenable);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_clear_io_count(ushort CardNo, ushort chan);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_io_count_value_extern(ushort CardNo, ushort chan, ref int current_value);
        //����
        [DllImport("LTDMC.dll")]
        public static extern short dmc_change_speed_extend(ushort CardNo, ushort axis,double Curr_Vel, double Taccdec, ushort pin_num, ushort trig_mode);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_follow_vector_speed_move(ushort CardNo, ushort axis, ushort Follow_AxisNum, ushort[] Follow_AxisList,double ratio);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_conti_line_unit_extend(ushort CardNo, ushort Crd, ushort AxisNum, ushort[] AxisList, double[] pPosList, ushort posi_mode, double Extend_Len, ushort enable, int mark); //�����岹ֱ��
     
        //���߲���
        [DllImport("LTDMC.dll", EntryPoint = "nmc_download_configfile", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short nmc_download_configfile(ushort CardNo, ushort PortNum, string FileName);//����ENI�����ļ�
        [DllImport("LTDMC.dll", EntryPoint = "nmc_download_mapfile", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short nmc_download_mapfile(ushort CardNo, string FileName);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_upload_configfile(ushort CardNo, ushort PortNum, string FileName);
        [DllImport("LTDMC.dll", EntryPoint = "nmc_set_manager_para", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short nmc_set_manager_para(ushort CardNo, ushort PortNum, int baudrate, ushort ManagerID);
        [DllImport("LTDMC.dll", EntryPoint = "nmc_get_manager_para", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short nmc_get_manager_para(ushort CardNo, ushort PortNum, ref uint baudrate, ref ushort ManagerID);
        [DllImport("LTDMC.dll", EntryPoint = "nmc_set_manager_od", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short nmc_set_manager_od(ushort CardNo, ushort PortNum, ushort index, ushort subindex, ushort valuelength, uint value);
        [DllImport("LTDMC.dll", EntryPoint = "nmc_get_manager_od", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short nmc_get_manager_od(ushort CardNo, ushort PortNum, ushort index, ushort subindex, ushort valuelength, ref uint value);

        [DllImport("LTDMC.dll", EntryPoint = "nmc_get_total_axes", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short nmc_get_total_axes(ushort CardNo, ref uint TotalAxis);
        [DllImport("LTDMC.dll", EntryPoint = "nmc_get_total_ionum", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short nmc_get_total_ionum(ushort CardNo, ref ushort TotalIn, ref ushort TotalOut);
        [DllImport("LTDMC.dll", EntryPoint = "nmc_get_LostHeartbeat_Nodes", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short nmc_get_LostHeartbeat_Nodes(ushort CardNo, ushort PortNum, ushort[] NodeID, ref ushort NodeNum);
        [DllImport("LTDMC.dll", EntryPoint = "nmc_get_EmergeneyMessege_Nodes", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short nmc_get_EmergeneyMessege_Nodes(ushort CardNo, ushort PortNum, uint[] NodeMsg, ref ushort MsgNum);
        [DllImport("LTDMC.dll", EntryPoint = "nmc_SendNmtCommand", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short nmc_SendNmtCommand(ushort CardNo, ushort PortNum, ushort NodeID, ushort NmtCommand);
        [DllImport("LTDMC.dll", EntryPoint = "nmc_syn_move", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern short nmc_syn_move(ushort CardNo, ushort AxisNum, ushort[] AxisList, int[] Position, ushort[] PosiMode);
        //
        [DllImport("LTDMC.dll")]
        public static extern short nmc_syn_move_unit(ushort CardNo, ushort AxisNum, ushort[] AxisList, double[] Position, ushort[] PosiMode);
        //���߶���ͬ���˶�
        [DllImport("LTDMC.dll")]
        public static extern short nmc_sync_pmove_unit(ushort CardNo, ushort AxisNum, ushort[] AxisList, double[] Dist, ushort[] PosiMode);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_sync_vmove_unit(ushort CardNo, ushort AxisNum, ushort[] AxisList, ushort[] Dir);
        //������վ����
        [DllImport("LTDMC.dll")]
        public static extern short nmc_set_master_para(ushort CardNo, ushort PortNum, ushort Baudrate, uint NodeCnt, ushort MasterId);
        //��ȡ��վ����
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_master_para(ushort CardNo, ushort PortNum, ref ushort Baudrate, ref uint NodeCnt, ref ushort MasterId);
        //��ȡ����ADDA�����������
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_total_adcnum(ushort CardNo, ref ushort TotalIn, ref ushort TotalOut);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_set_controller_workmode(ushort CardNo, ushort controller_mode);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_controller_workmode(ushort CardNo, ref ushort controller_mode);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_set_cycletime(ushort CardNo, ushort FieldbusType, int CycleTime);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_cycletime(ushort CardNo, ushort FieldbusType, ref int CycleTime);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_node_od(ushort CardNo, ushort PortNum, ushort nodenum, ushort index, ushort subindex, ushort valuelength, ref int value);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_set_node_od(ushort CardNo, ushort PortNum, ushort nodenum, ushort index, ushort subindex, ushort valuelength, int value);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_reset_to_factory(ushort CardNo, ushort PortNum, ushort NodeNum);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_set_alarm_clear(ushort CardNo, ushort PortNum, ushort nodenum);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_slave_nodes(ushort CardNo, ushort PortNum, ushort BaudRate, ref ushort NodeId, ref ushort NodeNum);
        
        //��״̬��
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_axis_state_machine(ushort CardNo, ushort axis, ref ushort Axis_StateMachine);
        //��ȡ��״̬��
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_axis_statusword(ushort CardNo, ushort axis, ref int statusword);
        //��ȡ�����ÿ���ģʽ������ֵ��6����ģʽ��8cspģʽ��
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_axis_setting_contrlmode(ushort CardNo, ushort axis, ref int contrlmode);
        //���������������
        [DllImport("LTDMC.dll")]
        public static extern short nmc_set_axis_contrlword(ushort CardNo, ushort axis, int contrlword);
        //��ȡ�����������
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_axis_contrlword(ushort CardNo, ushort axis, ref int contrlword);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_axis_type(ushort CardNo, ushort axis, ref ushort Axis_Type);
        //��ȡ����ʱ������ƽ��ʱ�䣬���ʱ�䣬ִ��������
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_consume_time_fieldbus(ushort CardNo, ushort Fieldbustype, ref uint Average_time, ref uint Max_time, ref ulong Cycles);
        //���ʱ����
        [DllImport("LTDMC.dll")]
        public static extern short nmc_clear_consume_time_fieldbus(ushort CardNo, ushort Fieldbustype);
        //���ߵ���ʹ�ܺ��� 255��ʾȫʹ��
        [DllImport("LTDMC.dll")]
        public static extern short nmc_set_axis_enable(ushort CardNo, ushort axis);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_set_axis_disable(ushort CardNo, ushort axis);
        // ��ȡ��Ĵ�վ��Ϣ
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_axis_node_address(ushort CardNo, ushort axis, ref ushort SlaveAddr, ref ushort Sub_SlaveAddr);
        //��ȡ��������
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_total_slaves(ushort CardNo, ushort PortNum, ref ushort TotalSlaves);
        [DllImport("LTDMC.dll")]
        //���߻�ԭ�㺯��
        public static extern short nmc_set_home_profile(ushort CardNo, ushort axis, ushort home_mode, double Low_Vel, double High_Vel, double Tacc, double Tdec, double offsetpos);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_home_profile(ushort CardNo, ushort axis, ref ushort home_mode, ref double Low_Vel, ref double High_Vel, ref double Tacc, ref double Tdec, ref double offsetpos);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_home_move(ushort CardNo, ushort axis);
        //
        [DllImport("LTDMC.dll")]
        public static extern short nmc_start_scan_ethercat(ushort CardNo, ushort AddressID);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_stop_scan_ethercat(ushort CardNo, ushort AddressID);
        //�����������ģʽ 1Ϊppģʽ��6Ϊ����ģʽ��8Ϊcspģʽ
        [DllImport("LTDMC.dll")]
        public static extern short nmc_set_axis_run_mode(ushort CardNo, ushort axis, ushort run_mode);
        //����˿ڱ���
        [DllImport("LTDMC.dll")]
        public static extern short nmc_clear_alarm_fieldbus(ushort CardNo, ushort PortNum);
        //ֹͣethercat����,����0��ʾ�ɹ�������������ʾ���ɹ�
        [DllImport("LTDMC.dll")]
        public static extern short nmc_stop_etc(ushort CardNo, ref ushort ETCState);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_axis_contrlmode(ushort CardNo, ushort Axis, ref int Contrlmode);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_axis_io_in(ushort CardNo, ushort axis);

        [DllImport("LTDMC.dll")]
        public static extern short nmc_set_axis_io_out(ushort CardNo, ushort axis, uint iostate);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_axis_io_out(ushort CardNo, ushort axis);
        // ��ȡ���߶˿ڴ�����
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_errcode(ushort CardNo, ushort channel, ref ushort errcode);
        // ������߶˿ڴ�����
        [DllImport("LTDMC.dll")]
        public static extern short nmc_clear_errcode(ushort CardNo, ushort channel);
        // ��ȡ�����������
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_axis_errcode(ushort CardNo, ushort axis, ref ushort Errcode);
        // ��������������
        [DllImport("LTDMC.dll")]
        public static extern short nmc_clear_axis_errcode(ushort CardNo, ushort axis);

        //RTEX�����Ӻ���
        [DllImport("LTDMC.dll")]
        public static extern short nmc_start_connect(ushort CardNo, ushort chan, ref ushort info, ref ushort len);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_vendor_info(ushort CardNo, ushort axis, byte[] info, ref ushort len);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_slave_type_info(ushort CardNo, ushort axis, byte[] info, ref ushort len);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_slave_name_info(ushort CardNo, ushort axis, byte[] info, ref ushort len);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_slave_version_info(ushort CardNo, ushort axis, byte[] info, ref ushort len);

        [DllImport("LTDMC.dll")]
        public static extern short nmc_write_parameter(ushort CardNo, ushort axis, ushort index, ushort subindex, uint para_data);
        /**************************************************************
        *����˵����RTEX������дEEPROM����
        **************************************************************/
        [DllImport("LTDMC.dll")]
        public static extern short nmc_write_slave_eeprom(ushort CardNo, ushort axis);

        [DllImport("LTDMC.dll")]
        public static extern short nmc_read_parameter(ushort CardNo, ushort axis, ushort index, ushort subindex, ref uint para_data);
        /**************************************************************
         * *index:rtex�������Ĳ�������
         * *subindex:rtex��������index����µĲ������
         * *para_data:�����Ĳ���ֵ
         * **************************************************************/
        [DllImport("LTDMC.dll")]
        public static extern short nmc_read_parameter_attributes(ushort CardNo, ushort axis, ushort index, ushort subindex, ref uint para_data);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_set_cmdcycletime(ushort CardNo, ushort PortNum, uint cmdtime);
        //����RTEX�������ڱ�(us)
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_cmdcycletime(ushort CardNo, ushort PortNum, ref uint cmdtime);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_config_atuo_log(ushort CardNo, ushort ifenable, ushort dir, ushort byte_index, ushort mask, ushort condition, uint counter);

        //��չPDO
        [DllImport("LTDMC.dll")]
        public static extern short nmc_write_rxpdo_extra(ushort CardNo, ushort PortNum, ushort address, ushort DataLen, int Value);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_read_rxpdo_extra(ushort CardNo, ushort PortNum, ushort address, ushort DataLen, ref int Value);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_read_txpdo_extra(ushort CardNo, ushort PortNum, ushort address, ushort DataLen, ref int Value);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_write_rxpdo_extra_uint(ushort CardNo, ushort PortNum, ushort address, ushort DataLen, uint Value);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_read_rxpdo_extra_uint(ushort CardNo, ushort PortNum, ushort address, ushort DataLen, ref uint Value);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_read_txpdo_extra_uint(ushort CardNo, ushort PortNum, ushort address, ushort DataLen, ref uint Value);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_log_state(ushort CardNo, ushort chan, ref uint state);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_driver_reset(ushort CardNo, ushort axis);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_set_offset_pos(ushort CardNo, ushort axis, double offset_pos);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_offset_pos(ushort CardNo, ushort axis, ref double offset_pos);
        //���rtex����ֵ�������Ķ�Ȧֵ
        [DllImport("LTDMC.dll")]
        public static extern short nmc_clear_abs_driver_multi_cycle(ushort CardNo, ushort axis);
        //---------------------------EtherCAT IO��չģ�����ָ��----------------------
        //����io���32λ������չ
        [DllImport("LTDMC.dll")]
        public static extern short nmc_write_outport_extern(ushort CardNo, ushort Channel, ushort NoteID, ushort portno, uint outport_val);
        //��ȡio���32λ������չ
        [DllImport("LTDMC.dll")]
        public static extern short nmc_read_outport_extern(ushort CardNo, ushort Channel, ushort NoteID, ushort portno, ref uint outport_val);
        //��ȡio����32λ������չ
        [DllImport("LTDMC.dll")]
        public static extern short nmc_read_inport_extern(ushort CardNo, ushort Channel, ushort NoteID, ushort portno, ref uint inport_val);
        //����io���
        [DllImport("LTDMC.dll")]
        public static extern short nmc_write_outbit_extern(ushort CardNo, ushort Channel, ushort NoteID, ushort IoBit, ushort IoValue);
        //��ȡio���
        [DllImport("LTDMC.dll")]
        public static extern short nmc_read_outbit_extern(ushort CardNo, ushort Channel, ushort NoteID, ushort IoBit, ref ushort IoValue);
        //��ȡio����
        [DllImport("LTDMC.dll")]
        public static extern short nmc_read_inbit_extern(ushort CardNo, ushort Channel, ushort NoteID, ushort IoBit, ref ushort IoValue);
        
        //�������������
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_current_fieldbus_state_info(ushort CardNo, ushort Channel, ref ushort Axis, ref ushort ErrorType, ref ushort SlaveAddr, ref uint ErrorFieldbusCode);
        // ������ʷ������
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_detail_fieldbus_state_info(ushort CardNo, ushort Channel, uint ReadErrorNum, ref uint TotalNum, ref uint ActualNum, ushort[] Axis, ushort[] ErrorType, ushort[] SlaveAddr, uint[] ErrorFieldbusCode);
        //�����ɼ�
        [DllImport("LTDMC.dll")]
        public static extern short nmc_start_pdo_trace(ushort CardNo, ushort Channel, ushort SlaveAddr, ushort Index_Num, uint Trace_Len, ushort[] Index, ushort[] Sub_Index);
        //��ȡ�ɼ�����
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_pdo_trace(ushort CardNo, ushort Channel, ushort SlaveAddr, ref ushort Index_Num, ref uint Trace_Len, ushort[] Index, ushort[] Sub_Index);
        //���ô����ɼ�����
        [DllImport("LTDMC.dll")]
        public static extern short nmc_set_pdo_trace_trig_para(ushort CardNo, ushort Channel, ushort SlaveAddr, ushort Trig_Index, ushort Trig_Sub_Index, int Trig_Value, ushort Trig_Mode);
        //��ȡ�����ɼ�����
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_pdo_trace_trig_para(ushort CardNo, ushort Channel, ushort SlaveAddr, ref ushort Trig_Index, ref ushort Trig_Sub_Index, ref int Trig_Value, ref ushort Trig_Mode);
        //�ɼ����
        [DllImport("LTDMC.dll")]
        public static extern short nmc_clear_pdo_trace_data(ushort CardNo, ushort Channel, ushort SlaveAddr);
        //�ɼ�ֹͣ
        [DllImport("LTDMC.dll")]
        public static extern short nmc_stop_pdo_trace(ushort CardNo, ushort Channel, ushort SlaveAddr);
        //�ɼ����ݶ�ȡ
        [DllImport("LTDMC.dll")]
        public static extern short nmc_read_pdo_trace_data(ushort CardNo, ushort Channel, ushort SlaveAddr, uint StartAddr, uint Readlen, ref uint ActReadlen, byte[] Data);
        //�Ѳɼ�����
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_pdo_trace_num(ushort CardNo, ushort Channel, ushort SlaveAddr, ref uint Data_num, ref uint Size_of_each_bag);
        //�ɼ�״̬
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_pdo_trace_state(ushort CardNo, ushort Channel, ushort SlaveAddr, ref ushort Trace_state);
        //����ר��
        [DllImport("LTDMC.dll")]
        public static extern short nmc_reset_canopen(ushort CardNo);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_reset_rtex(ushort CardNo);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_reset_etc(ushort CardNo);
        //���ߴ���������
        [DllImport("LTDMC.dll")]
        public static extern short nmc_set_fieldbus_error_switch(ushort CardNo, ushort channel, ushort data);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_fieldbus_error_switch(ushort CardNo, ushort channel, ref ushort data);

        ////����CST�л���CSP���������������ܼ�ʱͬ����վĿ��λ�ã���ʱʱ������վ����ͬ��������ʵ��λ�ã���ȡ���ù���
        //[DllImport("LTDMC.dll")]
        //public static extern short nmc_torque_set_delay_cycle(ushort CardNo, ushort axis, int delay_cycle);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_torque_move(ushort CardNo, ushort axis, int Torque, ushort PosLimitValid, double PosLimitValue, ushort PosMode);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_change_torque(ushort CardNo, ushort axis, int Torque);
        //��ȡת�ش�С
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_torque(ushort CardNo, ushort axis, ref int Torque);
        //modbus����
        [DllImport("LTDMC.dll")]
        public static extern short dmc_modbus_active_COM1(ushort id, string COMID,int speed, int bits, int check, int stop);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_modbus_active_COM2(ushort id, string COMID, int speed, int bits, int check, int stop);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_modbus_active_ETH(ushort id, ushort port);

        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_modbus_0x(ushort CardNo, ushort start, ushort inum, byte[] pdata);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_modbus_0x(ushort CardNo, ushort start, ushort inum, byte[] pdata);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_modbus_4x(ushort CardNo, ushort start, ushort inum, ushort[] pdata);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_modbus_4x(ushort CardNo, ushort start, ushort inum, ushort[] pdata);

        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_modbus_4x_float(ushort CardNo, ushort start, ushort inum, float[] pdata);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_modbus_4x_float(ushort CardNo, ushort start, ushort inum, float[] pdata);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_modbus_4x_int(ushort CardNo, ushort start, ushort inum, int[] pdata);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_modbus_4x_int(ushort CardNo, ushort start, ushort inum, int[] pdata);
        //����
        [DllImport("LTDMC.dll")]
        public static extern short dmc_conti_line_io_union(ushort CardNo, ushort Crd, ushort AxisNum, ushort[] AxisList,double[] pPosList, ushort posi_mode, ushort bitno, ushort on_off,double io_value, ushort io_mode, ushort MapAxis, ushort pos_source,double ReverseTime,long mark);
        //���ñ���������������DMC3000ϵ�����忨��
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_encoder_dir(ushort CardNo, ushort axis, ushort dir);
        
        //Բ����������λ��������DMC3000/5X10ϵ�����忨��
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_arc_zone_limit_config(ushort CardNo, ushort[] AxisList, ushort AxisNum, double[] Center, double Radius, ushort Source, ushort StopMode);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_arc_zone_limit_config(ushort CardNo, ushort[] AxisList, ref ushort AxisNum, double[] Center, ref double Radius, ref ushort Source, ref ushort StopMode);
        //Բ����������λunit��������DMC5X10ϵ�����忨��
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_arc_zone_limit_config_unit(ushort CardNo, ushort[] AxisList, ushort AxisNum, double[] Center, double Radius, ushort Source, ushort StopMode);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_arc_zone_limit_config_unit(ushort CardNo, ushort[] AxisList, ref ushort AxisNum, double[] Center, ref double Radius, ref ushort Source, ref ushort StopMode);
        //��ѯ��Ӧ���״̬��������DMC3000/5X10ϵ�����忨��
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_arc_zone_limit_axis_status(ushort CardNo, ushort axis);
        //Բ����λʹ�ܣ�������DMC3000/5X10ϵ�����忨��
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_arc_zone_limit_enable(ushort CardNo, ushort enable);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_arc_zone_limit_enable(ushort CardNo, ref ushort enable);
        
        //���ƿ����ߺж��ߺ��Ƿ��ʼ�������ƽ
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_output_status_repower(ushort CardNo, ushort enable);
        //�ɽӿڣ�������������ʹ��
        [DllImport("LTDMC.dll")]
        public static extern short dmc_t_pmove_extern_softlanding(ushort CardNo, ushort axis, double MidPos, double TargetPos, double start_Vel, double Max_Vel, double stop_Vel, uint delay_ms, double Max_Vel2, double stop_vel2, double acc_time, double dec_time, ushort posi_mode);
        //����
        [DllImport("LTDMC.dll")]
        public static extern short dmc_compare_add_point_XD(ushort CardNo, ushort cmp, long pos, ushort dir, ushort action, uint actpara, long startPos);//���綨�ƱȽϺ���
        
        //---------------------------ORG���봥�����߱��ٱ�λ----------------------
        //����ORG���봥�����߱��ٱ�λ��������DMC3000/5X10ϵ�����忨��
        [DllImport("LTDMC.dll")]
        public static extern short dmc_pmove_change_pos_speed_config(ushort CardNo, ushort axis, double tar_vel, double tar_rel_pos, ushort trig_mode, ushort source);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_pmove_change_pos_speed_config(ushort CardNo, ushort axis, ref double tar_vel, ref double tar_rel_pos, ref ushort trig_mode, ref ushort source);
        //ORG���봥�����߱��ٱ�λʹ�ܣ�������DMC3000/5X10ϵ�����忨��
        [DllImport("LTDMC.dll")]
        public static extern short dmc_pmove_change_pos_speed_enable(ushort CardNo, ushort axis, ushort enable);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_pmove_change_pos_speed_enable(ushort CardNo, ushort axis, ref ushort enable);
        //��ȡORG���봥�����߱��ٱ�λ��״̬  trig_num ����������trig_pos ����λ�ã�������DMC3000/5X10ϵ�����忨��
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_pmove_change_pos_speed_state(ushort CardNo, ushort axis, ref ushort trig_num, double[] trig_pos);
        //IO���ٱ�λ������io����ڣ�������EtherCAT����ϵ�п���
        [DllImport("LTDMC.dll")]
        public static extern short dmc_pmove_change_pos_speed_inbit(ushort CardNo, ushort axis, ushort inbit, ushort enable);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_pmove_change_pos_speed_inbit(ushort CardNo, ushort axis, ref ushort inbit, ref ushort enable);
        //����
        [DllImport("LTDMC.dll")]
        public static extern short dmc_compare_add_point_extend(ushort CardNo, ushort axis, long pos, ushort dir, ushort action, ushort para_num, ref uint actpara, uint compare_time);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_cmd_position(ushort CardNo, ushort axis, ref double pos);
        //�߼��������ã��ڲ�ʹ�ã�
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_logic_analyzer_config(ushort CardNo, ushort channel, uint SampleFre, uint SampleDepth, ushort SampleMode);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_start_logic_analyzer(ushort CardNo, ushort channel, ushort enable);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_logic_analyzer_counter(ushort CardNo, ushort channel, ref uint counter);
        //20190923�޸�kg���ƺ����ӿڣ��ͻ����ƣ�
        [DllImport("LTDMC.dll")]
        public static extern short dmc_read_inbit_append(ushort CardNo, ushort bitno);//��ȡ����ڵ�״̬
        [DllImport("LTDMC.dll")]
        public static extern short dmc_write_outbit_append(ushort CardNo, ushort bitno, ushort on_off);//��������ڵ�״̬
        [DllImport("LTDMC.dll")]
        public static extern short dmc_read_outbit_append(ushort CardNo, ushort bitno);//��ȡ����ڵ�״̬
        [DllImport("LTDMC.dll")]
        public static extern uint dmc_read_inport_append(ushort CardNo, ushort portno);//��ȡ����˿ڵ�ֵ
        [DllImport("LTDMC.dll")]
        public static extern uint dmc_read_outport_append(ushort CardNo, ushort portno);//��ȡ����˿ڵ�ֵ
        [DllImport("LTDMC.dll")]
        public static extern short dmc_write_outport_append(ushort CardNo, ushort portno, uint port_value);//������������˿ڵ�ֵ

        //---------------------------��Բ�岹���������----------------------
        // ��������ϵ������棨������DMC5X10ϵ�����忨��E5032���߿���
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_tangent_follow(ushort CardNo, ushort Crd, ushort axis, ushort follow_curve, ushort rotate_dir, double degree_equivalent);
        // ��ȡָ������ϵ������������������DMC5X10ϵ�����忨��E5032���߿���
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_tangent_follow_param(ushort CardNo, ushort Crd, ref ushort axis, ref ushort follow_curve, ref ushort rotate_dir, ref double degree_equivalent);
        // ȡ������ϵ���棨������DMC5X10ϵ�����忨��E5032���߿���
        [DllImport("LTDMC.dll")]
        public static extern short dmc_disable_follow_move(ushort CardNo, ushort Crd);
        // ��Բ�岹��������DMC5X10ϵ�����忨��E5032���߿���
        [DllImport("LTDMC.dll")]
        public static extern short dmc_ellipse_move(ushort CardNo, ushort Crd, ushort axisNum, ushort[] Axis_List, double[] Target_Pos, double[] Cen_Pos, double A_Axis_Len, double B_Axis_Len, ushort Dir, ushort Pos_Mode);

        //---------------------------���Ź�����----------------------
        //���ÿ��ſڴ�����Ӧ�¼���������DMC3000/5X10ϵ�����忨��
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_watchdog_action_event(ushort CardNo, ushort event_mask);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_watchdog_action_event(ushort CardNo, ref ushort event_mask);
        //ʹ�ܿ��ſڱ������ƣ�������DMC3000/5X10ϵ�����忨��
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_watchdog_enable(ushort CardNo, double timer_period, ushort enable);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_watchdog_enable(ushort CardNo, ref double timer_period, ref ushort enable);
        //��λ���Ź���ʱ����������DMC3000/5X10ϵ�����忨��
        [DllImport("LTDMC.dll")]
        public static extern short dmc_reset_watchdog_timer(ushort CardNo);

        //io���ƹ��ܣ������ࣩ
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_io_check_control(ushort CardNo, ushort sensor_in_no, ushort check_mode, ushort A_out_no, ushort B_out_no, ushort C_out_no, ushort output_mode);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_io_check_control(ushort CardNo, ref ushort sensor_in_no, ref ushort check_mode, ref ushort A_out_no, ref ushort B_out_no, ref ushort C_out_no, ref ushort output_mode);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_stop_io_check_control(ushort CardNo);

        //������λ����ƫ�ƾ��루������DMC3000/5X10ϵ�����忨��
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_el_ret_deviation(ushort CardNo, ushort axis, ushort enable, double deviation);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_el_ret_deviation(ushort CardNo, ushort axis, ref ushort enable, ref double deviation);

        //����λ�õ��ӣ����ٱȽϹ��ܣ�����ʹ�ã�
        [DllImport("LTDMC.dll")]
        public static extern short dmc_hcmp_set_config_overlap(ushort CardNo, ushort hcmp, ushort axis, ushort cmp_source, ushort cmp_logic, int time, ushort axis_num, ushort aux_axis, ushort aux_source);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_hcmp_get_config_overlap(ushort CardNo, ushort hcmp, ref ushort axis, ref ushort cmp_source, ref ushort cmp_logic, ref int time, ref ushort axis_num, ref ushort aux_axis, ref ushort aux_source);
        
        //�������߹ر�RTCP����,��������

        //�����岹(����ʹ�ã�DMC5000/5X10ϵ�����忨��E5032���߿�)
        [DllImport("LTDMC.dll")]
        public static extern short dmc_conti_helix_move_unit(ushort CardNo, ushort Crd, ushort AxisNum, ushort[] AixsList, double[] StartPos, double[] TargetPos, ushort Arc_Dir, int Circle, ushort mode, int mark);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_helix_move_unit(ushort CardNo, ushort Crd, ushort AxisNum, ushort[] AxisList, double[] StartPos, double[] TargetPos, ushort Arc_Dir, int Circle, ushort mode);

        //PDO����20190715���ڲ�ʹ�ã�
        [DllImport("LTDMC.dll")]
        public static extern short dmc_pdo_buffer_enter(ushort CardNo, ushort axis);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_pdo_buffer_stop(ushort CardNo, ushort axis);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_pdo_buffer_clear(ushort CardNo, ushort axis);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_pdo_buffer_run_state(ushort CardNo, ushort axis, ref int RunState, ref int Remain, ref int NotRunned, ref int Runned);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_pdo_buffer_add_data(ushort CardNo, ushort axis, int size, int[] data_table);       
        [DllImport("LTDMC.dll")]
        public static extern short dmc_pdo_buffer_start_multi(ushort CardNo, ushort AxisNum, ushort[] AxisList, ushort[] ResultList);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_pdo_buffer_pause_multi(ushort CardNo, ushort AxisNum, ushort[] AxisList, ushort[] ResultList);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_pdo_buffer_stop_multi(ushort CardNo, ushort AxisNum, ushort[] AxisList, ushort[] ResultList);
        //[DllImport("LTDMC.dll")]
        //public static extern short dmc_pdo_buffer_add_data_multi(UInt16 CardNo, UInt16 AxisNum, UInt16[] AxisList, int size, int[][] data_table);
        //����
        [DllImport("LTDMC.dll")]
        public static extern short dmc_calculate_arccenter_3point(double[] start_pos, double[] mid_pos, double[] target_pos, double[] cen_pos);

        //---------------------ָ��������˶�------------------
        //ָ��������˶���������DMC3000/5000ϵ�����忨��
        [DllImport("LTDMC.dll")]
        public static extern short dmc_m_set_muti_profile_unit(ushort card, ushort group, ushort axis_num, ushort[] axis_list, double[] start_vel, double[] max_vel, double[] tacc, double[] tdec, double[] stop_vel);//�����ٶ�����
        [DllImport("LTDMC.dll")]
        public static extern short dmc_m_set_profile_unit(ushort card, ushort group, ushort axis, double start_vel, double max_vel, double tacc, double tdec, double stop_vel);//�����ٶ�����
        [DllImport("LTDMC.dll")]
        public static extern short dmc_m_add_sigaxis_moveseg_data(ushort card, ushort group, ushort axis, double Target_pos, ushort process_mode, uint mark);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_m_add_sigaxis_move_twoseg_data(ushort card, ushort group, ushort axis, double Target_pos, double second_pos, double second_vel, double second_endvel, ushort process_mode, uint mark);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_m_add_mutiaxis_moveseg_data(ushort card, ushort group, ushort axisnum, ushort[] axis_list, double[] Target_pos, ushort process_mode, uint mark);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_m_add_mutiaxis_move_twoseg_data(ushort card, ushort group, ushort axisnum, ushort[] axis_list, double[] Target_pos, double[] second_pos, double[] second_vel, double[] second_endvel, ushort process_mode, uint mark);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_m_add_ioTrig_movseg_data(ushort card, ushort group, ushort axisNum, ushort[] axisList, double[] Target_pos, ushort process_mode, ushort trigINbit, ushort trigINstate, uint mark);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_m_add_mutiposTrig_movseg_data(ushort card, ushort group, ushort axis, double Target_pos, ushort process_mode, ushort trigaxisNum, ushort[] trigAxisList, double[] trigPos, ushort[] trigPosType, ushort[] trigMode, uint mark);//λ�ô����ƶ�
        [DllImport("LTDMC.dll")]
        public static extern short dmc_m_add_mutiposTrig_mov_twoseg_data(ushort card, ushort group, ushort axis, double Target_pos, double softland_pos, double softland_vel, double softland_endvel, ushort process_mode, ushort trigAxisNum, ushort[] trigAxisList, double[] trigPos, ushort[] trigPosType, ushort[] trigMode, uint mark);//����λ�ô����ƶ�
        [DllImport("LTDMC.dll")]
        public static extern short dmc_m_add_upseg_data(ushort card, ushort group, ushort axis, double Target_pos, uint mark);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_m_add_up_twoseg_data(ushort card, ushort group, ushort axis, double Target_pos, double second_pos, double second_vel, double second_endvel, uint mark);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_m_add_ioPosTrig_movseg_data(ushort card, ushort group, ushort axisNum, ushort[] axisList, double[] Target_pos, ushort process_mode, ushort trigAxis, double trigPos, ushort trigPosType, ushort trigMode, ushort TrigINNum, ushort[] trigINList, ushort[] trigINstate, uint mark);//λ��+io�����ƶ�
        [DllImport("LTDMC.dll")]
        public static extern short dmc_m_add_ioPosTrig_mov_twoseg_data(ushort card, ushort group, ushort axisNum, ushort[] axisList, double[] Target_pos, double[] second_pos, double[] second_vel, double[] second_endvel, ushort process_mode, ushort trigAxis, double trigPos, ushort trigPosType, ushort trigMode, ushort TrigINNum, ushort[] trigINList, ushort[] trigINstate, uint mark);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_m_add_posTrig_movseg_data(ushort card, ushort group, ushort axisNum, ushort[] axisList, double[] Target_pos, ushort process_mode, ushort trigAxis, double trigPos, ushort trigPosType, ushort trigMode, uint mark);//λ�ô����ƶ�
        [DllImport("LTDMC.dll")]
        public static extern short dmc_m_add_posTrig_mov_twoseg_data(ushort card, ushort group, ushort axisNum, ushort[] axisList, double[] Target_pos, double[] second_pos, double[] second_vel, double[] second_endvel, ushort process_mode, ushort trigAxis, double trigPos, ushort trigPosType, ushort trigMode, uint mark);//λ�ô����ƶ�
        [DllImport("LTDMC.dll")]
        public static extern short dmc_m_add_ioPosTrig_down_seg_data(ushort card, ushort group, ushort axis, double safePos, double Target_pos, ushort trigAxisNum, ushort[] trigAxisList, double[] trigPos, ushort[] trigPosType, ushort[] trigMode, ushort trigIN, ushort trigINstate, uint mark);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_m_add_ioPosTrig_down_twoseg_data(ushort card, ushort group, ushort axis, double safePos, double Target_pos, double second_pos, double second_vel, double second_endvel, ushort trigAxisNum, ushort[] trigAxisList, double[] trigPos, ushort[] trigPosType, ushort[] trigMode, ushort trigIN, ushort trigINstate, uint mark);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_m_add_posTrig_down_seg_data(ushort card, ushort group, ushort axis, double safePos, double Target_pos, ushort trigAxisNum, ushort[] trigAxisList, double[] trigPos, ushort[] trigPosType, ushort[] trigMode, uint mark);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_m_add_posTrig_down_twoseg_data(ushort card, ushort group, ushort axis, double safePos, double Target_pos, double second_pos, double second_vel, double second_endvel, ushort trigAxisNum, ushort[] trigAxisList, double[] trigPos, ushort[] trigPosType, ushort[] trigMode, uint mark);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_m_add_posTrig_down_seg_cmd_data(ushort card, ushort group, ushort axis, double safePos, double Target_pos, ushort trigAxisNum, ushort[] trigAxisList, uint mark);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_m_add_posTrig_down_twoseg_cmd_data(ushort card, ushort group, ushort axis, double safePos, double Target_pos, double second_pos, double second_vel, double second_endvel, ushort trigAxisNum, ushort[] trigAxisList, uint mark);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_m_add_mutiposTrig_singledown_seg_data(ushort card, ushort group, ushort axis, double safePos, double Target_pos, ushort process_mode, ushort trigAxisNum, ushort[] trigAxisList, double[] trigPos, ushort[] trigPosType, ushort[] trigMode, uint mark);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_m_add_mutiposTrig_mutidown_seg_data(ushort card, ushort group, ushort axisnum, ushort[] axis_list, double[] safePos, double[] Target_pos, ushort process_mode, ushort trigAxisNum, ushort[] trigAxisList, double[] trigPos, ushort[] trigPosType, ushort[] trigMode, uint mark);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_m_posTrig_outbit(ushort card, ushort group, ushort bitno, ushort on_off, ushort ahead_axis, double ahead_value, ushort ahead_PosType, ushort ahead_Mode, uint mark);//λ�ô���IO���
        [DllImport("LTDMC.dll")]
        public static extern short dmc_m_mutiposTrig_outbit(ushort card, ushort group, ushort bitno, ushort on_off, ushort process_mode, ushort trigaxisNum, ushort[] trigAxisList, double[] trigPos, ushort[] trigPosType, ushort[] trigMode, uint mark);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_m_immediate_write_outbit(ushort card, ushort group, ushort bitno, ushort on_off, uint mark);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_m_wait_input(ushort card, ushort group, ushort bitno, ushort on_off, double time_out, uint mark);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_m_delay_time(ushort card, ushort group, double delay_time, uint mark);//��ʱָ��
        [DllImport("LTDMC.dll")]
        public static extern short dmc_m_get_run_state(ushort card, ushort group, ref ushort state, ref ushort enable, ref uint stop_reason, ref ushort trig_phase, ref uint mark);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_m_open_list(ushort card, ushort group, ushort axis_num, ushort[] axis_list);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_m_close_list(ushort card, ushort group);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_m_start_list(ushort card, ushort group);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_m_stop_list(ushort card, ushort group, ushort stopMode);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_m_pause_list(ushort card, ushort group, ushort stopMode);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_m_set_encoder_error_allow(ushort card, ushort group, double allow_error);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_m_get_encoder_error_allow(ushort card, ushort group, ref double allow_error);

        //��ȡ����AD���루������DMC5X10ϵ�����忨��
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_ad_input_all(ushort CardNo, ref double Vout);
        //�����岹��ͣ��ʹ��pmove��������DMC5X10ϵ�����忨��
        [DllImport("LTDMC.dll")]
        public static extern short dmc_conti_pmove_unit_pausemode(ushort CardNo, ushort axis, double TargetPos, double Min_Vel, double Max_Vel, double stop_Vel, double acc, double dec, double smooth_time, ushort posi_mode);
        //�����岹��ͣʹ��pmove�󣬻ص���ͣλ�ã�������DMC5X10ϵ�����忨��
        [DllImport("LTDMC.dll")]
        public static extern short dmc_conti_return_pausemode(ushort CardNo, ushort Crd, ushort axis);
        //������ߺ��Ƿ�֧��ͨѶУ�飨������DMC3000/5X10ϵ�����忨��
        [DllImport("LTDMC.dll")]
        public static extern short dmc_check_if_crc_support(ushort CardNo);

        //����ײ��⹦�ܽӿ� ��������DMC3000ϵ�����忨��
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_axis_conflict_config(ushort CardNo, ushort[] axis_list, ushort[] axis_depart_dir, double home_dist, double conflict_dist, ushort stop_mode);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_axis_conflict_config(ushort CardNo, ushort[] axis_list, ushort[] axis_depart_dir, ref double home_dist, ref double conflict_dist, ref ushort stop_mode);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_axis_conflict_config_en(ushort CardNo, ushort enable);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_axis_conflict_config_en(ushort CardNo, ref ushort enable);
       
        //����ּ��ͨ��,�ּ�̼�ר��
        [DllImport("LTDMC.dll")]
        public static extern short dmc_sorting_close_ex(ushort CardNo, ushort sortModuleNo);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_sorting_start_ex(ushort CardNo, ushort sortModuleNo);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_sorting_set_init_config_ex(ushort CardNo, ushort cameraCount, int[] pCameraPos, ushort[] pCamIONo, uint cameraTime, ushort cameraTrigLevel, ushort blowCount, int[] pBlowPos, ushort[] pBlowIONo, uint blowTime, ushort blowTrigLevel, ushort axis, ushort dir, ushort checkMode, ushort sortModuleNo);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_sorting_set_camera_trig_count_ex(ushort CardNo, ushort cameraNum, uint cameraTrigCnt, ushort sortModuleNo);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_sorting_get_camera_trig_count_ex(ushort CardNo, ushort cameraNum, ref uint pCameraTrigCnt, ushort count, ushort sortModuleNo);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_sorting_set_blow_trig_count_ex(ushort CardNo, ushort blowNum, uint blowTrigCnt, ushort sortModuleNo);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_sorting_get_blow_trig_count_ex(ushort CardNo, ushort blowNum, ref uint pBlowTrigCnt, ushort count, ushort sortModuleNo);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_sorting_get_camera_config_ex(ushort CardNo, ushort index, ref int pos, ref uint trigTime, ref ushort ioNo, ref ushort trigLevel, ushort sortModuleNo);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_sorting_get_blow_config_ex(ushort CardNo, ushort index, ref int pos, ref uint trigTime, ref ushort ioNo, ref ushort trigLevel, ushort sortModuleNo);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_sorting_get_blow_status_ex(ushort CardNo, ref uint trigCntAll, ref ushort trigMore, ref ushort trigLess, ushort sortModuleNo);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_sorting_trig_blow_ex(ushort CardNo, ushort blowNum, ushort sortModuleNo);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_sorting_set_blow_enable_ex(ushort CardNo, ushort blowNum, ushort enable, ushort sortModuleNo);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_sorting_set_piece_config_ex(ushort CardNo, uint maxWidth, uint minWidth, uint minDistance, uint minTimeIntervel, ushort sortModuleNo);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_sorting_get_piece_status_ex(ushort CardNo, ref uint pieceFind, ref uint piecePassCam, ref uint dist2next, ref uint pieceWidth, ushort sortModuleNo);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_sorting_set_cam_trig_phase_ex(ushort CardNo, ushort blowNo, double coef, ushort sortModuleNo);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_sorting_set_blow_trig_phase_ex(ushort CardNo, ushort blowNo, double coef, ushort sortModuleNo);
        //��ȡ�ּ�ָ����������
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_sortdev_blow_cmd_cnt(ushort CardNo, ushort blowDevNum, ref long cnt);
        //��ȡδ����ָ��������������
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_sortdev_blow_cmderr_cnt(ushort CardNo, ushort blowDevNum, ref long errCnt);
        //�ּ����״̬
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_sortqueue_status(ushort CardNo, ref long curSorQueueLen, ref long passCamWithNoCmd);

        // ��Բ�����岹��������DMC5X10ϵ�����忨��E5032���߿���
        [DllImport("LTDMC.dll")]
        public static extern short dmc_conti_ellipse_move_unit(ushort CardNo, ushort Crd,ushort AxisNum, ushort[] AxisList, double[] Target_Pos, double[] Cen_Pos, double A_Axis_Len, double B_Axis_Len, ushort Dir, ushort Pos_Mode,long mark);
        //��ȡ��״̬������Ԥ����
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_axis_status_advance(ushort CardNo, ushort axis_no, ushort motion_no, ref ushort axis_plan_state, ref uint ErrPlulseCnt, ref ushort fpga_busy);
        //�����岹vmove��DMC5000ϵ�п�����ʹ�ã�
        [DllImport("LTDMC.dll")]
        public static extern short dmc_conti_vmove_unit(ushort CardNo, ushort Crd, ushort axis, double vel, double acc, ushort dir, int imark);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_conti_vmove_stop(ushort CardNo, ushort Crd, ushort axis, double dec, int imark);

        //---------------------��д���籣����------------------//
        //д���ַ����ݵ��ϵ籣������DMC3000/5000ϵ�п�����ʹ�ã�
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_persistent_reg_byte(ushort CardNo, uint start, uint inum, byte[] pdata);
        //�Ӷϵ籣������ȡд����ַ���DMC3000/5000ϵ�п�����ʹ�ã�
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_persistent_reg_byte(ushort CardNo, uint start, uint inum, byte[] pdata);
        //д�븡�������ݵ��ϵ籣������DMC3000/5000ϵ�п�����ʹ�ã�
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_persistent_reg_float(ushort CardNo, uint start, uint inum, float[] pdata);
        //�Ӷϵ籣������ȡд��ĸ��������ݣ�DMC3000/5000ϵ�п�����ʹ�ã�
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_persistent_reg_float(ushort CardNo, uint start, uint inum, float[] pdata);
        //д���������ݵ��ϵ籣������DMC3000/5000ϵ�п�����ʹ�ã�
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_persistent_reg_int(ushort CardNo, uint start, uint inum, int[] pdata);
        //�Ӷϵ籣������ȡд����������ݣ�DMC3000/5000ϵ�п�����ʹ�ã�
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_persistent_reg_int(ushort CardNo, uint start, uint inum, int[] pdata);
        //----------------------------------------------------//

        //EtherCAT���߸�λIOģ��������ֿ�������202001������������EtherCAT���߿���
        [DllImport("LTDMC.dll")]
        public static extern short nmc_set_slave_output_retain(ushort CardNo, ushort Enable);
        [DllImport("LTDMC.dll")]
        public static extern short nmc_get_slave_output_retain(ushort CardNo, ref ushort Enable);

        //���������дflash��ʵ�ֶϵ籣�漱ͣ�ź����ã�������DMC3000ϵ�����忨��
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_persistent_param_config(ushort CardNo, ushort axis, uint item);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_persistent_param_config(ushort CardNo, ushort axis, ref uint item);               

        //��ȡ����ʱ�����������̼����Ǳ��ݹ̼���������DMC3000/5000/5X10ϵ�����忨��
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_firmware_boot_type(ushort CardNo, ref ushort boot_type);

        /**************************�жϹ��� ��������DMC5X10ϵ�����忨��************************/
        //�������ƿ��жϹ���
        [DllImport("LTDMC.dll")]
        public static extern uint dmc_int_enable(ushort CardNo, DMC3K5K_OPERATE funcIntHandler, IntPtr operate_data);
        //��ֹ���ƿ����ж�
        [DllImport("LTDMC.dll")]
        public static extern uint dmc_int_disable(ushort CardNo);
        //����/��ȡָ�����ƿ��ж�ͨ��ʹ��
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_intmode_enable(ushort Cardno,ushort Intno,ushort Enable);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_intmode_enable(ushort Cardno,ushort Intno,ref ushort Status);
        //����/��ȡָ�����ƿ��ж�����
        [DllImport("LTDMC.dll")]
        public static extern short dmc_set_intmode_config(ushort Cardno,ushort Intno,ushort IntItem,ushort IntIndex,ushort IntSubIndex,ushort Logic);
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_intmode_config(ushort Cardno,ushort Intno,ref ushort IntItem,ref ushort IntIndex,ref ushort IntSubIndex,ref ushort Logic);
        //��ȡָ�����ƿ��ж�ͨ�����ж�״̬
        [DllImport("LTDMC.dll")]
        public static extern short dmc_get_int_status(ushort Cardno,ref uint IntStatus);
        //��λָ�����ƿ�����ڵ��ж�
        [DllImport("LTDMC.dll")]
        public static extern short dmc_reset_int_status(ushort Cardno, ushort Intno);
        /**************************************************************************************/
    }
}
